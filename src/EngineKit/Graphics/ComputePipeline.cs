namespace EngineKit.Graphics;

public sealed class ComputePipeline : Pipeline
{
    public ComputePipeline(ComputePipelineDescriptor computePipelineDescriptor)
    {
        ShaderProgram = new ShaderProgram(
            computePipelineDescriptor.ComputeShaderFilePath,
            computePipelineDescriptor.PipelineProgramLabel);
    }
}