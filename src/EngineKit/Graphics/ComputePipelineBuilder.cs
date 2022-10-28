using System.IO;
using CSharpFunctionalExtensions;

namespace EngineKit.Graphics;

internal sealed class ComputePipelineBuilder : IComputePipelineBuilder
{
    private readonly IInternalGraphicsContext _internalGraphicsContext;
    private ComputePipelineDescriptor _computePipelineDescriptor;
    private string? _computeShaderFilePath;
    private string? _computeShaderSource;
    private bool _shaderFromFile = false;

    public ComputePipelineBuilder(IInternalGraphicsContext internalGraphicsContext)
    {
        _internalGraphicsContext = internalGraphicsContext;
        _computePipelineDescriptor = new ComputePipelineDescriptor();
    }

    public IComputePipelineBuilder WithShaderFromFile(string computeShaderFilePath)
    {
        _computeShaderFilePath = computeShaderFilePath;
        _shaderFromFile = true;
        return this;
    }

    public IComputePipelineBuilder WithShaderFromSource(string computeShaderSource)
    {
        _computeShaderSource = computeShaderSource;
        _shaderFromFile = false;
        return this;
    }

    public Result<IComputePipeline> Build(Label label)
    {
        if (_shaderFromFile)
        {
            if (!File.Exists(_computeShaderFilePath))
            {
                return Result.Failure<IComputePipeline>($"File {_computeShaderFilePath} not found");
            }

            _computeShaderSource = File.ReadAllText(_computeShaderFilePath);
        }
        else
        {
            if (string.IsNullOrEmpty(_computeShaderSource))
            {
                return Result.Failure<IComputePipeline>($"Compute shader source not provided");
            }
        }

        _computePipelineDescriptor.ComputeShaderSource = _computeShaderSource;
        _computePipelineDescriptor.PipelineProgramLabel = label;
        return _internalGraphicsContext.CreateComputePipeline(_computePipelineDescriptor);
    }
}