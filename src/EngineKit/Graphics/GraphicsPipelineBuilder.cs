using CSharpFunctionalExtensions;

namespace EngineKit.Graphics;

internal sealed class GraphicsPipelineBuilder : IGraphicsPipelineBuilder
{
    private readonly IInternalGraphicsContext _internalGraphicsContext;
    private GraphicsPipelineDescriptor _graphicsPipelineDescriptor;

    public GraphicsPipelineBuilder(IInternalGraphicsContext internalGraphicsContext)
    {
        _internalGraphicsContext = internalGraphicsContext;
        _graphicsPipelineDescriptor = new GraphicsPipelineDescriptor
        {
            InputAssembly = new InputAssemblyDescriptor
            {
                IsPrimitiveRestartEnabled = false,
                PrimitiveTopology = PrimitiveTopology.Triangles
            },
            RasterizationDescriptor = new RasterizationDescriptor
            {
                CullMode = CullMode.Back,
                FaceWinding = FaceWinding.CounterClockwise,
                FillMode = FillMode.Solid,
                PointSize = 1.0f,
                LineWidth = 1.0f
            },
            ColorBlendDescriptor = new ColorBlendDescriptor
            {
                BlendConstants = new[] { 0.0f, 0.0f, 0.0f, 0.0f },
                ColorBlendAttachmentDescriptors = new[]
                {
                    ColorBlendAttachmentDescriptor.Opaque
                }
            },
            DepthStencilDescriptor = new DepthStencilDescriptor
            {
                DepthCompareOperation = CompareOperation.Less,
                IsDepthTestEnabled = true,
                IsDepthWriteEnabled = true
            }
        };
    }

    public IGraphicsPipelineBuilder WithVertexBindings(params VertexBindingDescriptor[] vertexBindings)
    {
        _graphicsPipelineDescriptor.VertexInput = new VertexInputDescriptor(vertexBindings);
        return this;
    }

    public IGraphicsPipelineBuilder WithVertexBindingsForVertexType(VertexType vertexType)
    {
        _graphicsPipelineDescriptor.VertexInput = VertexInputDescriptor.CreateFromVertexType(vertexType);
        return this;
    }

    public IGraphicsPipelineBuilder WithShaders(
        string vertexShaderFilePath,
        string fragmentShaderFilePath)
    {
        _graphicsPipelineDescriptor.VertexShaderFilePath = vertexShaderFilePath;
        _graphicsPipelineDescriptor.FragmentShaderFilePath = fragmentShaderFilePath;
        return this;
    }

    public IGraphicsPipelineBuilder DisableCulling()
    {
        _graphicsPipelineDescriptor.RasterizationDescriptor.IsCullingEnabled = false;
        return this;
    }

    public IGraphicsPipelineBuilder EnableCulling(CullMode cullMode)
    {
        _graphicsPipelineDescriptor.RasterizationDescriptor.IsCullingEnabled = true;
        _graphicsPipelineDescriptor.RasterizationDescriptor.CullMode = cullMode;
        return this;
    }

    public IGraphicsPipelineBuilder WithFillMode(FillMode fillMode)
    {
        _graphicsPipelineDescriptor.RasterizationDescriptor.FillMode = fillMode;
        return this;
    }

    public IGraphicsPipelineBuilder WithFaceWinding(FaceWinding faceWinding)
    {
        _graphicsPipelineDescriptor.RasterizationDescriptor.FaceWinding = faceWinding;
        return this;
    }

    public IGraphicsPipelineBuilder WithTopology(
        PrimitiveTopology primitiveTopology,
        bool isPrimitiveRestartEnabled = false)
    {
        _graphicsPipelineDescriptor.InputAssembly.PrimitiveTopology = primitiveTopology;
        _graphicsPipelineDescriptor.InputAssembly.IsPrimitiveRestartEnabled = isPrimitiveRestartEnabled;
        return this;
    }

    public IGraphicsPipelineBuilder EnableDepthTest()
    {
        _graphicsPipelineDescriptor.DepthStencilDescriptor.IsDepthTestEnabled = true;
        return this;
    }

    public IGraphicsPipelineBuilder EnableDepthWrite()
    {
        _graphicsPipelineDescriptor.DepthStencilDescriptor.IsDepthWriteEnabled = true;
        return this;
    }

    public IGraphicsPipelineBuilder DisableDepthTest()
    {
        _graphicsPipelineDescriptor.DepthStencilDescriptor.IsDepthTestEnabled = false;
        return this;
    }

    public IGraphicsPipelineBuilder DisableDepthWrite()
    {
        _graphicsPipelineDescriptor.DepthStencilDescriptor.IsDepthWriteEnabled = false;
        return this;
    }

    public IGraphicsPipelineBuilder UseDepthComparison(CompareOperation compareOperation)
    {
        _graphicsPipelineDescriptor.DepthStencilDescriptor.DepthCompareOperation = compareOperation;
        return this;
    }

    public Result<IGraphicsPipeline> Build(string label)
    {
        _graphicsPipelineDescriptor.PipelineProgramLabel = new Label(label);
        return _internalGraphicsContext.CreateGraphicsPipeline(_graphicsPipelineDescriptor);
    }
}