using System.Numerics;
using EngineKit;
using EngineKit.Graphics;
using EngineKit.Mathematics;
using ImGuiNET;
using Serilog;

namespace Complex;

internal class ForwardRenderer : IRenderer
{
    private readonly IApplicationContext _applicationContext;

    private readonly ICamera _camera;

    private readonly IGraphicsContext _graphicsContext;

    private readonly ILogger _logger;

    private readonly ISamplerLibrary _samplerLibrary;

    private uint _aabbCounter;

    private GpuGlobals _gpuGlobals;

    private IBuffer? _cameraInformationBuffer;

    private IGraphicsPipeline? _forwardGraphicsPipeline;

    private FramebufferDescriptor _forwardRenderPass;

    private ITexture? _forwardRenderPassColorAttachment;

    private ITexture? _forwardRenderPassDepthAttachment;

    private IBuffer? _indexBuffer;

    private IBuffer? _indirectBuffer;

    private IBuffer? _instanceBuffer;

    private bool _isLoaded;

    private IGraphicsPipeline? _lineRendererGraphicsPipeline;

    private IBuffer? _lineVertexBuffer;

    private IBuffer? _materialBuffer;

    private IMaterialPool? _materialPool;

    private readonly uint _maxAabbCount;

    private uint _meshInstanceCount;

    private IMeshPool? _meshPool;

    private Vector3 _uColor;

    private IBuffer? _vertexBuffer;

    public bool ShowAaBb;

    private bool _isEditor;

    public ForwardRenderer(
        ILogger logger,
        IGraphicsContext graphicsContext,
        ISamplerLibrary samplerLibrary,
        IApplicationContext applicationContext,
        ICamera camera)
    {
        _logger = logger;
        _graphicsContext = graphicsContext;
        _samplerLibrary = samplerLibrary;
        _applicationContext = applicationContext;
        _camera = camera;
        _uColor = new Vector3(0.61f,
            0.875f,
            0.85f);

        _maxAabbCount = 20_000;
        _aabbCounter = 0;
    }

    public FramebufferDescriptor GetMainFramebufferDescriptor()
    {
        return _forwardRenderPass;
    }

    public bool Load()
    {
        if (_isLoaded)
        {
            return _isLoaded;
        }

        _meshPool = _graphicsContext.CreateMeshPool("Vertices",
                10_000_000,
                7_500_000);
        _vertexBuffer = _meshPool.VertexBuffer;
        _indexBuffer = _meshPool.IndexBuffer;
        _materialPool = _graphicsContext.CreateMaterialPool("Materials",
                1024,
                _samplerLibrary);
        _materialBuffer = _materialPool.MaterialBuffer;

        _cameraInformationBuffer = _graphicsContext.CreateTypedBuffer<GpuGlobals>("GpuGlobals",
                1,
                BufferStorageFlags.DynamicStorage);
        _indirectBuffer = _graphicsContext.CreateTypedBuffer<DrawElementIndirectCommand>("GpuIndirectElements",
                20_480u,
                BufferStorageFlags.DynamicStorage);
        _instanceBuffer = _graphicsContext.CreateTypedBuffer<GpuInstance>("GpuInstances",
                20_480u,
                BufferStorageFlags.DynamicStorage);

        //TODO: refactor this out into some LineRenderer
        _lineVertexBuffer = _graphicsContext.CreateTypedBuffer<VertexPositionColor>("Debug-Aabb-Lines",
                _maxAabbCount,
                BufferStorageFlags.DynamicStorage);

        var forwardGraphicsPipelineResult = _graphicsContext.CreateGraphicsPipelineBuilder()
            .WithShadersFromFiles("Shaders/Simple.vs.glsl",
                "Shaders/Simple.fs.glsl")
            .WithTopology(PrimitiveTopology.Triangles)
            .WithVertexAttributesFromDescriptor(
                new VertexInputDescriptorBuilder()
                    .AddAttribute(0, Format.R32G32B32Float, 0)
                    .AddAttribute(0, Format.R32G32B32Float, 12)
                    .AddAttribute(0, Format.R32G32Float, 24)
                    .AddAttribute(0, Format.R32G32B32A32Float, 30)
                    .Build(nameof(VertexPositionNormalUvTangent)))
            .WithDepthTestEnabled(CompareFunction.Greater)
            .WithClipControlDepth(ClipControlDepth.ZeroToOne)
            .WithCullingEnabled(CullMode.Back)
            .WithFaceWinding(FaceWinding.CounterClockwise)
            .WithDepthTestEnabled(CompareFunction.Greater)
            .Build("ForwardPipeline");

        if (forwardGraphicsPipelineResult.IsFailure)
        {
            _logger.Error("{Category} {ErrorMessage}", "Renderer", forwardGraphicsPipelineResult.Error);
            return false;
        }

        _forwardGraphicsPipeline = forwardGraphicsPipelineResult.Value;

        //TODO(deccer) refactor this out into some LineRenderer
        var lineRendererGraphicsPipelineResult = _graphicsContext.CreateGraphicsPipelineBuilder()
            .WithShadersFromFiles("Shaders/Debug/Line.vs.glsl",
                "Shaders/Debug/Line.fs.glsl")
            .WithVertexAttributesFromDescriptor(
                new VertexInputDescriptorBuilder()
                    .AddAttribute(0, Format.R32G32B32Float, 0)
                    .AddAttribute(0, Format.R32G32B32Float, 12)
                    .Build(nameof(VertexPositionColor)))
            .WithTopology(PrimitiveTopology.Lines)
            .WithFaceWinding(FaceWinding.Clockwise)
            .WithClipControlDepth(ClipControlDepth.ZeroToOne)
            .WithDepthTestEnabled(CompareFunction.Greater)
            .WithBlendingEnabled(ColorBlendAttachmentDescriptor.Additive)
            .ClearResourceBindingsOnBind()
            .Build("Debug-Lines");

        if (lineRendererGraphicsPipelineResult.IsFailure)
        {
            _logger.Error("{Category} {ErrorMessage}", "Renderer", lineRendererGraphicsPipelineResult.Error);
            return false;
        }

        _lineRendererGraphicsPipeline = lineRendererGraphicsPipelineResult.Value;

        CreateFramebufferDependentResources();

        _isLoaded = true;

        return true;
    }

    public void Clear()
    {
        _meshInstanceCount = 0;
        _aabbCounter = 0;
    }

    public void AddMeshInstance(MeshPrimitive meshPrimitive,
                                Material material,
                                Matrix4x4 transform,
                                BoundingBox transformedMeshAabb)
    {
        var meshId = _meshPool!.GetOrAdd(meshPrimitive);
        var materialId = _materialPool!.GetOrAdd(material);

        _indirectBuffer!.UpdateElement(new DrawElementIndirectCommand
            {
                BaseInstance = 0,
                BaseVertex = meshId.VertexOffset,
                FirstIndex = meshId.IndexOffset,
                IndexCount = meshId.IndexCount,
                InstanceCount = 1
            },
            _meshInstanceCount);
        _instanceBuffer!.UpdateElement(new GpuInstance
            {
                WorldMatrix = transform,
                MaterialIndex = new UInt4(materialId.Index,
                    0,
                    0,
                    0)
            },
            _meshInstanceCount);

        if (ShowAaBb && _aabbCounter <= _maxAabbCount)
        {
            AddDebugLinesForBoundingBox(transformedMeshAabb);
        }

        _meshInstanceCount++;
    }

    public void Render(ICamera camera)
    {
        _gpuGlobals.ProjectionMatrix = camera.ProjectionMatrix;
        _gpuGlobals.ViewMatrix = camera.ViewMatrix;
        _cameraInformationBuffer!.UpdateElement(_gpuGlobals, 0);

        _graphicsContext.BeginRenderPass(_forwardRenderPass);
        _graphicsContext.BindGraphicsPipeline(_forwardGraphicsPipeline!);
        _forwardGraphicsPipeline!.VertexUniform(0, _uColor);

        _forwardGraphicsPipeline.BindAsVertexBuffer(_vertexBuffer!, 0, _meshPool!.VertexBufferStride, Offset.Zero);
        _forwardGraphicsPipeline.BindAsIndexBuffer(_indexBuffer!);
        _forwardGraphicsPipeline.BindAsUniformBuffer(_cameraInformationBuffer, 0, Offset.Zero, SizeInBytes.Whole);
        _forwardGraphicsPipeline.BindAsShaderStorageBuffer(_instanceBuffer!, 1, Offset.Zero, SizeInBytes.Whole);
        _forwardGraphicsPipeline.BindAsShaderStorageBuffer(_materialBuffer!, 2, Offset.Zero, SizeInBytes.Whole);
        if (_meshInstanceCount > 0)
        {
            _forwardGraphicsPipeline.MultiDrawElementsIndirect(_indirectBuffer!, _meshInstanceCount);
        }

        if (ShowAaBb && _aabbCounter > 0)
        {
            //TODO(deccer) refactor out into some sort of LineRenderer
            _graphicsContext.BindGraphicsPipeline(_lineRendererGraphicsPipeline!);
            _lineRendererGraphicsPipeline!.BindAsVertexBuffer(_lineVertexBuffer!, 0, VertexPositionColor.Stride, Offset.Zero);
            _lineRendererGraphicsPipeline.BindAsUniformBuffer(_cameraInformationBuffer, 0, Offset.Zero, SizeInBytes.Whole);
            _lineRendererGraphicsPipeline.DrawArrays(24 * _aabbCounter, Offset.Zero);
        }

        if (!_isEditor)
        {
            _graphicsContext.BlitFramebufferToSwapchain(
                _applicationContext.WindowScaledFramebufferSize.X,
                _applicationContext.WindowScaledFramebufferSize.Y,
                _applicationContext.WindowFramebufferSize.X,
                _applicationContext.WindowFramebufferSize.Y);
        }

        _graphicsContext.EndRenderPass();
    }

    public void RenderUI()
    {
        if (ImGui.Begin("Render Debug"))
        {
            if (ImGui.SliderFloat3("Colors", ref _uColor, 0.01f, 2.0f))
            {
            }

            ImGui.Checkbox("Show AABBs", ref ShowAaBb);

            ImGui.TextUnformatted($"Mesh Instance Count {_meshInstanceCount}");
        }

        ImGui.End();
    }

    public void Dispose()
    {
        DestroyFramebufferDependentResources();

        _lineRendererGraphicsPipeline?.Dispose();
        _forwardGraphicsPipeline?.Dispose();

        _cameraInformationBuffer?.Dispose();
        _indirectBuffer?.Dispose();
        _instanceBuffer?.Dispose();
        _meshPool?.Dispose();
        _materialPool?.Dispose();
    }

    //TODO(deccer) refactor this out to some LineRenderer
    private void AddDebugLinesForBoundingBox(BoundingBox boundingBox)
    {
        var nearColor = Colors.Orange.ToVector3();
        var farColor = Colors.DarkOrange.ToVector3();
        var bbCorners = boundingBox.GetCorners();

        var nearBottomRight = bbCorners[0];
        var nearTopRight = bbCorners[1];
        var nearTopLeft = bbCorners[2];
        var nearBottomLeft = bbCorners[3];

        var farBottomRight = bbCorners[4];
        var farTopRight = bbCorners[5];
        var farTopLeft = bbCorners[6];
        var farBottomLeft = bbCorners[7];

        VertexPositionColor[] vertices =
        [
            new VertexPositionColor(nearBottomRight, nearColor),
            new VertexPositionColor(nearTopRight, nearColor),

            new VertexPositionColor(nearTopRight, nearColor),
            new VertexPositionColor(nearTopLeft, nearColor),

            new VertexPositionColor(nearTopLeft, nearColor),
            new VertexPositionColor(nearBottomLeft, nearColor),

            new VertexPositionColor(nearBottomLeft, nearColor),
            new VertexPositionColor(nearBottomRight, nearColor),

            new VertexPositionColor(nearBottomRight, nearColor),
            new VertexPositionColor(farBottomRight, farColor),

            new VertexPositionColor(nearTopRight, nearColor),
            new VertexPositionColor(farTopRight, farColor),

            new VertexPositionColor(nearTopLeft, nearColor),
            new VertexPositionColor(farTopLeft, farColor),

            new VertexPositionColor(nearBottomLeft, nearColor),
            new VertexPositionColor(farBottomLeft, farColor),

            new VertexPositionColor(farBottomRight, farColor),
            new VertexPositionColor(farTopRight, farColor),

            new VertexPositionColor(farTopRight, farColor),
            new VertexPositionColor(farTopLeft, farColor),

            new VertexPositionColor(farTopLeft, farColor),
            new VertexPositionColor(farBottomLeft, farColor),

            new VertexPositionColor(farBottomLeft, farColor),
            new VertexPositionColor(farBottomRight, farColor)
        ];

        _lineVertexBuffer!.UpdateElements(vertices, _aabbCounter * 24);
        _aabbCounter++;
    }

    public void ResizeIfNecessary()
    {
        if (!_applicationContext.HasWindowFramebufferSizeChanged && !_applicationContext.HasSceneViewSizeChanged)
        {
            return;
        }

        ResizeFramebufferDependentResources();

        if (!_isEditor)
        {
            _camera.Resize(_applicationContext.WindowFramebufferSize.X, _applicationContext.WindowFramebufferSize.Y);
            _forwardRenderPass.ResizeViewport(_applicationContext.WindowFramebufferSize.X, _applicationContext.WindowFramebufferSize.Y);
        }
        else
        {
            _camera.Resize(_applicationContext.SceneViewSize.X, _applicationContext.SceneViewSize.Y);
            _forwardRenderPass.ResizeViewport(_applicationContext.SceneViewSize.X, _applicationContext.SceneViewSize.Y);
        }

    }

    private void ResizeFramebufferDependentResources()
    {
        if (_applicationContext.HasWindowFramebufferSizeChanged || _applicationContext.HasSceneViewSizeChanged)
        {
            DestroyFramebufferDependentResources();
            CreateFramebufferDependentResources();

            _applicationContext.HasSceneViewSizeChanged = false;
            _applicationContext.HasWindowFramebufferSizeChanged = false;
        }
    }

    private void CreateFramebufferDependentResources()
    {
        _forwardRenderPassColorAttachment = _graphicsContext.CreateTexture2D(_isEditor
                ? _applicationContext.SceneViewScaledSize
                : _applicationContext.WindowScaledFramebufferSize,
            Format.R8G8B8A8Srgb,
            "ForwardColorAttachment");
        _forwardRenderPassDepthAttachment = _graphicsContext.CreateTexture2D(_isEditor
            ? _applicationContext.SceneViewScaledSize
            : _applicationContext.WindowScaledFramebufferSize,
            Format.D32Float,
            "ForwardDepthAttachment");

        var viewport = _isEditor
            ? _applicationContext.SceneViewScaledSize
            : _applicationContext.WindowFramebufferSize;

        _forwardRenderPass = _graphicsContext.GetFramebufferDescriptorBuilder()
            .WithColorAttachment(_forwardRenderPassColorAttachment,
                true,
                Colors.DarkSlateBlue) //MathHelper.GammaToLinear(Colors.DarkSlateBlue))
            .WithDepthAttachment(_forwardRenderPassDepthAttachment,
                true,
                0)
            .WithViewport(viewport.X, viewport.Y)
            .Build("ForwardRenderPass");
    }

    private void DestroyFramebufferDependentResources()
    {
        _forwardRenderPassColorAttachment?.Dispose();
        _forwardRenderPassDepthAttachment?.Dispose();
        _graphicsContext.RemoveFramebuffer(_forwardRenderPass);
    }
}
