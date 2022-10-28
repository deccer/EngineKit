namespace EngineKit.Graphics;

internal record struct ComputePipelineDescriptor
{
    public Label PipelineProgramLabel;

    public string ComputeShaderSource;
}