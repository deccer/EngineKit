using System.Reflection.Emit;

namespace EngineKit.Graphics;

public record struct ComputePipelineDescriptor
{
    public Label PipelineProgramLabel;

    public string ComputeShaderFilePath;
}