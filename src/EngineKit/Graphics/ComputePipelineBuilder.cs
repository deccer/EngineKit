using CSharpFunctionalExtensions;

namespace EngineKit.Graphics;

internal sealed class ComputePipelineBuilder : IComputePipelineBuilder
{
    private readonly IInternalGraphicsContext _internalGraphicsContext;
    private ComputePipelineDescriptor _computePipelineDescriptor;

    public ComputePipelineBuilder(IInternalGraphicsContext internalGraphicsContext)
    {
        _internalGraphicsContext = internalGraphicsContext;
        _computePipelineDescriptor = new ComputePipelineDescriptor();
    }

    public IComputePipelineBuilder WithShaders(string computeShaderFilePath)
    {
        _computePipelineDescriptor.ComputeShaderFilePath = computeShaderFilePath;
        return this;
    }

    public Result<IComputePipeline> Build(Label label)
    {
        _computePipelineDescriptor.PipelineProgramLabel = label;
        return _internalGraphicsContext.CreateComputePipeline(_computePipelineDescriptor);
    }
}