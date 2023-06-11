using System.Collections.Generic;
using EngineKit;
using EngineKit.Graphics;
using EngineKit.Native.OpenGL;
using EngineKit.Mathematics;
using Serilog;
using SpaceGame.Game;
using SpaceGame.Game.Ecs;

namespace SpaceGame;

internal sealed class ForwardRenderer : IRenderer
{
    private readonly ILogger _logger;
    private readonly IApplicationContext _applicationContext;
    private readonly IGraphicsContext _graphicsContext;
    private readonly IRendererContext _rendererContext;

    private ITexture? _forwardFramebufferColorTexture;
    private ITexture? _forwardFramebufferDepthTexture;
    private ISampler? _textureSampler;

    private IGraphicsPipeline? _forwardSolidPipeline;
    private IGraphicsPipeline? _forwardWireframePipeline;

    private FramebufferRenderDescriptor _forwardFramebufferRenderDescriptor;

    private GpuBaseInformation _gpuBaseInformation;
    private IUniformBuffer? _gpuBaseInformationBuffer;

    private IVertexBuffer? _vertexBuffer;
    private IIndexBuffer? _indexBuffer;
    private IShaderStorageBuffer? _instanceBuffer;
    private IIndirectBuffer? _indirectBuffer;
    private IShaderStorageBuffer? _materialBuffer;
    private IReadOnlyCollection<ITexture>? _textureArrays;
    private IUniformBuffer _lightInformationBuffer;

    public ForwardRenderer(
        ILogger logger,
        IApplicationContext applicationContext,
        IGraphicsContext graphicsContext,
        IRendererContext rendererContext)
    {
        _logger = logger;
        _applicationContext = applicationContext;
        _graphicsContext = graphicsContext;
        _rendererContext = rendererContext;
        _gpuBaseInformation = new GpuBaseInformation();
    }

    public bool Load()
    {
        _gpuBaseInformationBuffer = _graphicsContext.CreateUniformBuffer<GpuBaseInformation>("BaseInformation");
        _gpuBaseInformationBuffer.AllocateStorage(_gpuBaseInformation, StorageAllocationFlags.Dynamic);

        var solidForwardPipelineResult = _graphicsContext.CreateGraphicsPipelineBuilder()
            .EnableDepthTest()
            .WithTopology(PrimitiveTopology.Triangles)
            .WithVertexInput(new VertexInputDescriptorBuilder()
                .AddAttribute(0, DataType.Float, 3, 0)
                .AddAttribute(0, DataType.Float, 3, 12)
                .AddAttribute(0, DataType.Float, 2, 24)
                .AddAttribute(0, DataType.Float, 4, 32)
                .Build("SolidForward_PositionNormalUvTangent"))
            .WithFaceWinding(FaceWinding.Clockwise)
            .WithShadersFromFiles("Shaders/Forward.vs.glsl", "Shaders/Forward.fs.glsl")
            .Build("SolidForwardPipeline");
        if (solidForwardPipelineResult.IsFailure)
        {
            _logger.Error("Unable to create 'SolidForward_PositionNormalUvTangent' graphics pipeline {Details}", solidForwardPipelineResult.Error);
            return false;
        }

        _forwardSolidPipeline = solidForwardPipelineResult.Value;

        var wireframeForwardPipelineResult = _graphicsContext.CreateGraphicsPipelineBuilder()
            .EnableCulling(CullMode.Back)
            .WithTopology(PrimitiveTopology.Triangles)
            .WithFillMode(FillMode.Line)
            .WithVertexInput(new VertexInputDescriptorBuilder()
                .AddAttribute(0, DataType.Float, 3, 0)
                .Build("WireframeForward_Position"))
            .WithFaceWinding(FaceWinding.CounterClockwise)
            .WithShadersFromFiles("Shaders/Forward.vs.glsl", "Shaders/Forward.fs.glsl")
            .Build("WireframeForwardPipeline");
        if (wireframeForwardPipelineResult.IsFailure)
        {
            _logger.Error("Unable to create 'WireframeForward_Position' graphics pipeline {Details}", wireframeForwardPipelineResult.Error);
            return false;
        }

        _forwardWireframePipeline = wireframeForwardPipelineResult.Value;

        var samplerDescriptor = new SamplerDescriptor
        {
            MagFilter = Filter.Linear,
            MinFilter = Filter.Linear,
            MipmapFilter = Filter.Linear,
            MaxLod = 10,
            AddressModeU = AddressMode.Repeat,
            AddressModeV = AddressMode.Repeat
        };
        _textureSampler = _graphicsContext.CreateSampler(samplerDescriptor);

        CreateSizeDependentResources();

        return true;
    }

    public void Dispose()
    {
        DestroySizeDependentResources();
        _textureSampler?.Dispose();

        _forwardSolidPipeline?.Dispose();
        _forwardWireframePipeline?.Dispose();
    }

    public void PrepareScene(EntityWorld entityWorld)
    {

    }

    public bool RenderScene(ICamera camera, Vector3 directionalLightPosition)
    {
        _gpuBaseInformation.ViewToClipMatrix = camera.ProjectionMatrix;
        _gpuBaseInformation.ClipToViewMatrix = Matrix.Invert(camera.ProjectionMatrix);
        _gpuBaseInformation.WorldToViewMatrix = camera.ViewMatrix;
        _gpuBaseInformation.ViewToWorldMatrix = Matrix.Invert(camera.ViewMatrix);
        _gpuBaseInformation.CameraPosition = new Vector4(camera.Position, 0.0f);
        _gpuBaseInformation.CameraDirection = new Vector4(camera.Direction, 0.0f);

        _gpuBaseInformationBuffer!.Update(_gpuBaseInformation, 0);

        GL.PushDebugGroup("Forward");
        {
            if (_rendererContext.UseWireframe)
            {
                if (_forwardWireframePipeline == null ||
                    !_graphicsContext.BindGraphicsPipeline(_forwardWireframePipeline))
                {
                    // ReSharper disable once StructuredMessageTemplateProblem
                    _logger.Error("Application - Unable to bind graphics pipeline {Name}",
                        nameof(_forwardWireframePipeline));
                    return false;
                }
            }
            else
            {
                if (_forwardSolidPipeline == null || !_graphicsContext.BindGraphicsPipeline(_forwardSolidPipeline))
                {
                    // ReSharper disable once StructuredMessageTemplateProblem
                    _logger.Error("Application - Unable to bind graphics pipeline {Name}",
                        nameof(_forwardSolidPipeline));
                    return false;
                }
            }

            var pipeline = _rendererContext.UseWireframe ? _forwardWireframePipeline : _forwardSolidPipeline;

            _graphicsContext.BeginRenderToFramebuffer(_forwardFramebufferRenderDescriptor);
            {
                pipeline!.BindUniformBuffer(
                    _gpuBaseInformationBuffer!,
                    1);
                pipeline.BindShaderStorageBuffer(
                    _instanceBuffer!,
                    2);

                pipeline.BindVertexBuffer(
                    _vertexBuffer!,
                    0,
                    0);
                pipeline.BindIndexBuffer(_indexBuffer!);

                pipeline.BindUniformBuffer(_lightInformationBuffer, 3);

                if (_materialBuffer != null)
                {
                    pipeline.BindShaderStorageBuffer(_materialBuffer!, 4);
                    var bindingIndex = 0u;
                    foreach (var textureArray in _textureArrays!)
                    {
                        pipeline.BindSampledTexture(_textureSampler!, textureArray, bindingIndex++);
                    }
                }

                //pipeline.BindShaderStorageBuffer(_gpuLightBuffer!, 5);

                pipeline.MultiDrawElementsIndirect(_indirectBuffer!, _indirectBuffer.Count);

                _graphicsContext.EndRender();
            }
            GL.PopDebugGroup();
        }

        return true;
    }

    public void RenderShadowDebugUi()
    {

    }

    public void CreateShadowMaps(int width, int height)
    {
    }

    public void Resize()
    {
        DestroySizeDependentResources();
        CreateSizeDependentResources();
    }

    private void DestroySizeDependentResources()
    {
        _forwardFramebufferColorTexture?.Dispose();
        _forwardFramebufferDepthTexture?.Dispose();
    }

    private void CreateSizeDependentResources()
    {
        _forwardFramebufferColorTexture = _graphicsContext.CreateTexture2D(_applicationContext.ScaledFramebufferSize.X, _applicationContext.ScaledFramebufferSize.Y, Format.R8G8B8UNorm);
        _forwardFramebufferDepthTexture =
            _graphicsContext.CreateTexture2D(_applicationContext.ScaledFramebufferSize.X, _applicationContext.ScaledFramebufferSize.Y, Format.D24UNormS8UInt);

        _forwardFramebufferRenderDescriptor = new FramebufferRenderDescriptorBuilder()
            .WithViewport(_applicationContext.ScaledFramebufferSize.X, _applicationContext.ScaledFramebufferSize.Y)
            .WithColorAttachment(_forwardFramebufferColorTexture, true, new Vector4(0.2f, 0.2f, 0.2f, 1.0f))
            //.WithDepthAttachment(_forwardFramebufferDepthTexture, true)
            .Build("Forward");
    }
}