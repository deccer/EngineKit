using System.Collections.Generic;
using System.IO;
using CSharpFunctionalExtensions;
using EngineKit.Graphics.Shaders;

namespace EngineKit.Graphics;

internal sealed class ComputePipelineBuilder : IComputePipelineBuilder
{
    private readonly IDictionary<IPipeline, ComputePipelineDescriptor> _computePipelineCache;
    private readonly IShaderProgramFactory _shaderProgramFactory;
    private ComputePipelineDescriptor _computePipelineDescriptor;
    private string? _computeShaderFilePath;
    private string? _computeShaderSource;
    private bool _shaderFromFile;

    public ComputePipelineBuilder(
        IDictionary<IPipeline, ComputePipelineDescriptor> computePipelineCache,
        IShaderProgramFactory shaderProgramFactory)
    {
        _computePipelineCache = computePipelineCache;
        _shaderProgramFactory = shaderProgramFactory;
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

    public IComputePipelineBuilder ClearResourceBindingsOnBind()
    {
        _computePipelineDescriptor.ClearResourceBindings = true;
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

        var computeShaderProgram = _shaderProgramFactory.CreateShaderProgram(
            _computePipelineDescriptor.PipelineProgramLabel,
            _computePipelineDescriptor.ComputeShaderSource);
        var computeShaderProgramLinkResult = computeShaderProgram.Link();
        if (computeShaderProgramLinkResult.IsFailure)
        {
            return Result.Failure<IComputePipeline>(computeShaderProgramLinkResult.Error);
        }

        var computePipeline = new ComputePipeline(_computePipelineDescriptor, computeShaderProgram);
        _computePipelineCache[computePipeline] = _computePipelineDescriptor;

        return Result.Success<IComputePipeline>(computePipeline);        
    }
}