using CSharpFunctionalExtensions;
using EngineKit.Core;

namespace EngineKit.Graphics;

public interface IComputePipelineBuilder
{
    Result<IComputePipeline> Build(Label label);

    IComputePipelineBuilder WithShaderFromFile(string computeShaderFilePath);

    IComputePipelineBuilder WithShaderFromSource(string computeShaderSource);

    IComputePipelineBuilder ClearResourceBindingsOnBind();
}
