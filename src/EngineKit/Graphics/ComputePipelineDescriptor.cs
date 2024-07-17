using EngineKit.Core;

namespace EngineKit.Graphics;

internal record struct ComputePipelineDescriptor
{
    public Label PipelineProgramLabel;

    public string ComputeShaderSource;

    public bool ClearResourceBindings;
}