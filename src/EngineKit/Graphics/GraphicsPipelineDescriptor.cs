namespace EngineKit.Graphics;

internal record struct GraphicsPipelineDescriptor
{
    public Label PipelineProgramLabel;

    public string VertexShaderSource;

    public string FragmentShaderSource;

    public InputAssemblyDescriptor InputAssembly;

    public VertexInputDescriptor VertexInput;

    public RasterizationDescriptor RasterizationDescriptor;

    public DepthStencilDescriptor DepthStencilDescriptor;

    public ColorBlendDescriptor ColorBlendDescriptor;
}