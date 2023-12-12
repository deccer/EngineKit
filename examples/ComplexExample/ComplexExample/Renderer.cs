using System.Numerics;
using System.Runtime.InteropServices;
using ComplexExample.Extensions;
using EngineKit;
using EngineKit.Graphics;
using EngineKit.Mathematics;
using EngineKit.Native.OpenGL;
using ImGuiNET;
using Serilog;

namespace ComplexExample;

internal sealed class Renderer : IRenderer
{
    private const int MegaByte = 1024 * 1024;
    
    private readonly ILogger _logger;
    private readonly IGraphicsContext _graphicsContext;
    private readonly IApplicationContext _applicationContext;
    private readonly ISamplerLibrary _samplerLibrary;
    private IMeshPool? _meshPool;
    private IMaterialPool? _materialPool;

    //private readonly IList<GpuObjectData> _objectData;
    private IBuffer? _geometryInstanceBuffer;
    private IBuffer? _geometryDrawIndirectBuffer;
    private int _objectDataIndex;
    private bool _isLoaded;

    private IComputePipeline? _cullObjectComputePipeline;

    private ITexture? _geometryAlbedoTexture;
    private ITexture? _geometryNormalTexture;
    private ITexture? _geometryArmTexture;
    private ITexture? _geometryEmissiveTexture;
    private ITexture? _geometryDepthTexture;
    private FramebufferDescriptor? _geometryFramebuffer;
    private IGraphicsPipeline? _geometryGraphicsPipeline;

    private CameraInformation _cameraInformation;
    private IBuffer? _cameraInformationBuffer;

    public Renderer(
        ILogger logger,
        IGraphicsContext graphicsContext,
        IApplicationContext applicationContext,
        ISamplerLibrary samplerLibrary)
    {
        _logger = logger;
        _graphicsContext = graphicsContext;
        _applicationContext = applicationContext;
        _samplerLibrary = samplerLibrary;

        //_objectData = new List<GpuObjectData>(16_384);
        _cameraInformation = new CameraInformation();
    }

    public void Dispose()
    {
        _materialPool?.Dispose();
        _meshPool?.Dispose();
        _cullObjectComputePipeline?.Dispose();

        DestroySizeDependentResources();
        _geometryGraphicsPipeline?.Dispose();
        
        _cameraInformationBuffer?.Dispose();
        _geometryInstanceBuffer?.Dispose();
        
    }

    public MeshId AddMeshPrimitive(MeshPrimitive meshPrimitive)
    {
        return _meshPool.GetOrAdd(meshPrimitive);
    }

    public MaterialId AddMaterial(Material material)
    {
        return _materialPool.GetOrAdd(material);
    }
    
    public void AddToRenderQueue(MeshId meshId, MaterialId materialId, Matrix4x4 worldMatrix)
    {
        if (!_isLoaded || _geometryInstanceBuffer == null || _geometryDrawIndirectBuffer == null)
        {
            return;
        }
        
        _geometryInstanceBuffer.Update(new GpuMeshInstance
        {
            WorldMatrix = worldMatrix,
            MaterialId = new Int4(materialId.Index, 0, 0, 0)
        }, _objectDataIndex);

        _geometryDrawIndirectBuffer.Update(new DrawElementIndirectCommand
        {
            FirstIndex = meshId.IndexOffset,
            IndexCount = meshId.IndexCount,
            BaseInstance = 0,
            BaseVertex = meshId.VertexOffset,
            InstanceCount = 1
        }, _objectDataIndex);

        _objectDataIndex++;
    }

    public void RenderWorld(ICamera camera)
    {
        if (_cameraInformationBuffer == null)
        {
            return;
        }
        
        GL.PushDebugGroup("Pass: GBuffer");
        
        _cameraInformation.ProjectionMatrix = camera.ProjectionMatrix;
        _cameraInformation.ViewMatrix = camera.ViewMatrix;
        _cameraInformation.Viewport = new Vector4(
            0,
            0,
            _applicationContext.ScaledFramebufferSize.X,
            _applicationContext.ScaledFramebufferSize.Y);
        _cameraInformation.CameraPosition = new Vector4(camera.Position, 0.0f);
        _cameraInformation.CameraDirection = new Vector4(camera.Direction, 0.0f);
        _cameraInformationBuffer.Update(_cameraInformation);

        //_graphicsContext.BindComputePipeline(_cullObjectComputePipeline);
        //_cullObjectComputePipeline.Dispatch(1, 1, 1);

        if (_geometryGraphicsPipeline == null || !_geometryFramebuffer.HasValue)
        {
            return;
        }

        _graphicsContext.BindGraphicsPipeline(_geometryGraphicsPipeline);
        _graphicsContext.BeginRenderPass(_geometryFramebuffer.Value);
        _geometryGraphicsPipeline.BindAsVertexBuffer(_meshPool.VertexBuffer, 0, 0);
        _geometryGraphicsPipeline.BindAsIndexBuffer(_meshPool.IndexBuffer);
        _geometryGraphicsPipeline.BindAsUniformBuffer(_cameraInformationBuffer, 0);
        _geometryGraphicsPipeline.BindAsShaderStorageBuffer(_geometryInstanceBuffer, 1);
        _geometryGraphicsPipeline.BindAsShaderStorageBuffer(_materialPool.MaterialBuffer, 2);
        if (_objectDataIndex > 0)
        {
            _geometryGraphicsPipeline.MultiDrawElementsIndirect(_geometryDrawIndirectBuffer, _objectDataIndex);
        }
        _graphicsContext.EndRenderPass();
        
        GL.PopDebugGroup();
    }

    public void ClearRenderQueue()
    {
        _objectDataIndex = 0;
    }

    public void CreateSizeDependentResources()
    {
        _geometryAlbedoTexture = _graphicsContext.CreateTexture2D(_applicationContext.ScaledFramebufferSize.X,
            _applicationContext.ScaledFramebufferSize.Y, Format.R8G8B8A8UNorm);
        _geometryNormalTexture = _graphicsContext.CreateTexture2D(_applicationContext.ScaledFramebufferSize.X,
            _applicationContext.ScaledFramebufferSize.Y, Format.R16G16B16A16Float);
        _geometryArmTexture = _graphicsContext.CreateTexture2D(_applicationContext.ScaledFramebufferSize.X,
            _applicationContext.ScaledFramebufferSize.Y, Format.R8G8B8A8UNorm);
        _geometryEmissiveTexture = _graphicsContext.CreateTexture2D(_applicationContext.ScaledFramebufferSize.X,
            _applicationContext.ScaledFramebufferSize.Y, Format.R16G16B16A16Float);
        _geometryDepthTexture = _graphicsContext.CreateTexture2D(_applicationContext.ScaledFramebufferSize.X,
            _applicationContext.ScaledFramebufferSize.Y, Format.D32Float);
        
        _geometryFramebuffer = new FramebufferDescriptorBuilder()
            .WithColorAttachment(_geometryAlbedoTexture, true, Vector4.Zero)
            .WithColorAttachment(_geometryNormalTexture, true, Vector4.Zero)
            .WithColorAttachment(_geometryArmTexture, true, Vector4.Zero)
            .WithColorAttachment(_geometryEmissiveTexture, true, Vector4.Zero)
            .WithDepthAttachment(_geometryDepthTexture, true, 1.0f)
            .WithViewport(_applicationContext.ScaledFramebufferSize.X, _applicationContext.ScaledFramebufferSize.Y)
            .Build("Geometry-Framebuffer");
    }

    public void DestroySizeDependentResources()
    {
        _geometryAlbedoTexture?.Dispose();
        _geometryNormalTexture?.Dispose();
        _geometryArmTexture?.Dispose();
        _geometryEmissiveTexture?.Dispose();
        _geometryDepthTexture?.Dispose();
        if (_geometryFramebuffer.HasValue)
        {
            _graphicsContext.RemoveFramebuffer(_geometryFramebuffer.Value);
        }
    }

    public bool Load()
    {
        /*
        var cullObjectComputePipelineResult = _graphicsContext.CreateComputePipelineBuilder()
            .WithShaderFromFile("Shaders/CullObjects.cs.glsl")
            .Build("CullObject-Pipeline");
        if (cullObjectComputePipelineResult.IsFailure)
        {
            _logger.Error("{Category} - Creating Culling Pipeline Failed - {Details}", "Renderer", cullObjectComputePipelineResult.Error);
            return false;
        }

        _cullObjectComputePipeline = cullObjectComputePipelineResult.Value;
        */

        var geometryGraphicsPipelineResult = _graphicsContext.CreateGraphicsPipelineBuilder()
            .WithShadersFromFiles("Shaders/Geometry.vs.glsl", "Shaders/Geometry.fs.glsl")
            .WithVertexAttributesFromDescriptor(VertexInputDescriptor.ForVertexType(VertexType.PositionNormalUvTangent))
            .WithTopology(PrimitiveTopology.Triangles)
            .WithFaceWinding(FaceWinding.CounterClockwise)
            .WithCullingEnabled(CullMode.Back)
            .WithDepthTestEnabled()
            .WithBlendingEnabled(ColorBlendAttachmentDescriptor.PreMultiplied)
            .Build("Geometry-Pipeline");

        if (geometryGraphicsPipelineResult.IsFailure)
        {
            _logger.Error("{Category} - Creating Geometry Pipeline Failed - {Details}", "Renderer", geometryGraphicsPipelineResult.Error);
            return false;
        }

        _geometryGraphicsPipeline = geometryGraphicsPipelineResult.Value;

        CreateSizeDependentResources();
        
        _geometryInstanceBuffer = _graphicsContext.CreateShaderStorageBuffer<GpuMeshInstance>("Instances");
        _geometryInstanceBuffer.AllocateStorage(Marshal.SizeOf<GpuMeshInstance>() * 4_096, StorageAllocationFlags.Dynamic);

        _geometryDrawIndirectBuffer = _graphicsContext.CreateDrawIndirectBuffer("SceneIndirects");
        _geometryDrawIndirectBuffer.AllocateStorage(4096 * Marshal.SizeOf<DrawElementIndirectCommand>(), StorageAllocationFlags.Dynamic);
        
        _meshPool = _graphicsContext.CreateMeshPool("Vertices", 1_024 * MegaByte, 768 * MegaByte);
        _materialPool = _graphicsContext.CreateMaterialPool("Materials", 16 * MegaByte, _samplerLibrary);

        _cameraInformationBuffer = _graphicsContext.CreateUniformBuffer<CameraInformation>("CameraInformation");
        _cameraInformationBuffer.AllocateStorage(Marshal.SizeOf<CameraInformation>(), StorageAllocationFlags.Dynamic);

        _isLoaded = true;
        return _isLoaded;
    }

    public void ShowScene()
    {
        var sceneViewSize = ImGui.GetContentRegionAvail();
        ImGuiExtensions.ShowImage(_geometryAlbedoTexture, sceneViewSize);
    }
}