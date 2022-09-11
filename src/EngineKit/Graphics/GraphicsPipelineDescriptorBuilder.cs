using System.Reflection.Emit;

namespace EngineKit.Graphics;

internal sealed class GraphicsPipelineDescriptorBuilder : IGraphicsPipelineDescriptorBuilder
{
    private GraphicsPipelineDescriptor _graphicsPipelineDescriptor;

    public GraphicsPipelineDescriptorBuilder()
    {
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

    public IGraphicsPipelineDescriptorBuilder WithVertexBindings(params VertexBindingDescriptor[] vertexBindings)
    {
        _graphicsPipelineDescriptor.VertexInput = new VertexInputDescriptor(vertexBindings);
        return this;
    }

    public IGraphicsPipelineDescriptorBuilder WithVertexBindingsForVertexType(VertexType vertexType)
    {
        _graphicsPipelineDescriptor.VertexInput = VertexInputDescriptor.CreateFromVertexType(vertexType);
        return this;
    }

    public IGraphicsPipelineDescriptorBuilder WithShaders(
        string vertexShaderFilePath,
        string fragmentShaderFilePath)
    {
        _graphicsPipelineDescriptor.VertexShaderFilePath = vertexShaderFilePath;
        _graphicsPipelineDescriptor.FragmentShaderFilePath = fragmentShaderFilePath;
        return this;
    }

    public IGraphicsPipelineDescriptorBuilder DisableCulling()
    {
        _graphicsPipelineDescriptor.RasterizationDescriptor.IsCullingEnabled = false;
        return this;
    }

    public IGraphicsPipelineDescriptorBuilder EnableCulling(CullMode cullMode)
    {
        _graphicsPipelineDescriptor.RasterizationDescriptor.IsCullingEnabled = true;
        _graphicsPipelineDescriptor.RasterizationDescriptor.CullMode = cullMode;
        return this;
    }

    public IGraphicsPipelineDescriptorBuilder WithFillMode(FillMode fillMode)
    {
        _graphicsPipelineDescriptor.RasterizationDescriptor.FillMode = fillMode;
        return this;
    }

    public IGraphicsPipelineDescriptorBuilder WithFaceWinding(FaceWinding faceWinding)
    {
        _graphicsPipelineDescriptor.RasterizationDescriptor.FaceWinding = faceWinding;
        return this;
    }

    public IGraphicsPipelineDescriptorBuilder WithTopology(
        PrimitiveTopology primitiveTopology,
        bool isPrimitiveRestartEnabled = false)
    {
        _graphicsPipelineDescriptor.InputAssembly.PrimitiveTopology = primitiveTopology;
        _graphicsPipelineDescriptor.InputAssembly.IsPrimitiveRestartEnabled = isPrimitiveRestartEnabled;
        return this;
    }

    public IGraphicsPipelineDescriptorBuilder EnableDepthTest()
    {
        _graphicsPipelineDescriptor.DepthStencilDescriptor.IsDepthTestEnabled = true;
        return this;
    }

    public IGraphicsPipelineDescriptorBuilder EnableDepthWrite()
    {
        _graphicsPipelineDescriptor.DepthStencilDescriptor.IsDepthWriteEnabled = true;
        return this;
    }

    public IGraphicsPipelineDescriptorBuilder DisableDepthTest()
    {
        _graphicsPipelineDescriptor.DepthStencilDescriptor.IsDepthTestEnabled = false;
        return this;
    }

    public IGraphicsPipelineDescriptorBuilder DisableDepthWrite()
    {
        _graphicsPipelineDescriptor.DepthStencilDescriptor.IsDepthWriteEnabled = false;
        return this;
    }

    public IGraphicsPipelineDescriptorBuilder UseDepthComparison(CompareOperation compareOperation)
    {
        _graphicsPipelineDescriptor.DepthStencilDescriptor.DepthCompareOperation = compareOperation;
        return this;
    }

    public GraphicsPipelineDescriptor Build(string label)
    {
        _graphicsPipelineDescriptor.PipelineProgramLabel = new Label(label);
        return _graphicsPipelineDescriptor;
    }
}