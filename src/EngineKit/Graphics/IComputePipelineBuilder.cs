using CSharpFunctionalExtensions;

namespace EngineKit.Graphics;

public interface IComputePipelineBuilder
{
    Result<IComputePipeline> Build(Label label);

    IComputePipelineBuilder WithShader(string computeShaderFilePath);
}