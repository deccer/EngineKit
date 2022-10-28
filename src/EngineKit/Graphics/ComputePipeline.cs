namespace EngineKit.Graphics;

internal sealed class ComputePipeline : Pipeline, IComputePipeline
{
    internal ComputePipeline(ComputePipelineDescriptor computePipelineDescriptor)
    {
        ShaderProgram = new ShaderProgram(
            computePipelineDescriptor.ComputeShaderSource,
            computePipelineDescriptor.PipelineProgramLabel);
    }
}