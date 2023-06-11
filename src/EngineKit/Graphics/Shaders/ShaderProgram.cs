using System;
using CSharpFunctionalExtensions;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics.Shaders;

public class ShaderProgram : IDisposable
{
    private readonly string? _computeShaderSource;
    private readonly string? _vertexShaderSource;
    private readonly string? _fragmentShaderSource;
    private readonly Label _label;

    public uint ProgramPipelineId;

    public Shader? VertexShader { get; private set; }

    public Shader? FragmentShader { get; private set; }

    public Shader? ComputeShader { get; private set; }

    public ShaderProgram(Label label, string computeShaderSource)
    {
        _label = label;
        _computeShaderSource = computeShaderSource ?? throw new ArgumentNullException(nameof(computeShaderSource));
        _vertexShaderSource = null!;
        _fragmentShaderSource = null!;
    }

    public ShaderProgram(
        Label label,
        string? vertexShaderSource,
        string? fragmentShaderSource)
    {
        _label = label;
        _computeShaderSource = null!;
        _vertexShaderSource = vertexShaderSource ?? throw new ArgumentNullException(nameof(vertexShaderSource));
        _fragmentShaderSource = fragmentShaderSource ?? throw new ArgumentNullException(nameof(fragmentShaderSource));
    }

    public Result Link()
    {
        ProgramPipelineId = GL.CreateProgramPipeline();
        GL.ObjectLabel(GL.ObjectIdentifier.ProgramPipeline, ProgramPipelineId, "ProgramPipeline-" + _label);

        var compilationResult = CreateShaders();
        if (compilationResult.IsFailure)
        {
            return compilationResult;
        }

        if (ComputeShader != null!)
        {
            compilationResult = ComputeShader.Compile();
            if (compilationResult.IsFailure)
            {
                return compilationResult;
            }

            GL.UseProgramStages(
                ProgramPipelineId,
                GL.UseProgramStageMask.ComputeShaderBit,
                ComputeShader.Id);

            return Result.Success();
        }

        if (VertexShader == null!)
        {
            return Result.Failure("No VertexShader available");
        }

        compilationResult = VertexShader.Compile();
        if (compilationResult.IsFailure)
        {
            return compilationResult;
        }

        if (FragmentShader == null!)
        {
            return Result.Failure("No FragmentShader available");
        }

        compilationResult = FragmentShader.Compile();
        if (compilationResult.IsFailure)
        {
            return compilationResult;
        }

        GL.UseProgramStages(ProgramPipelineId, GL.UseProgramStageMask.VertexShaderBit, VertexShader.Id);
        GL.UseProgramStages(ProgramPipelineId, GL.UseProgramStageMask.FragmentShaderBit, FragmentShader.Id);

        return Result.Success();
    }

    public void Use()
    {
        if (ComputeShader != null)
        {
            GL.UseProgramStages(ProgramPipelineId, GL.UseProgramStageMask.ComputeShaderBit, ComputeShader.Id);
        }
        else
        {
            GL.UseProgramStages(ProgramPipelineId, GL.UseProgramStageMask.VertexShaderBit, VertexShader!.Id);
            GL.UseProgramStages(ProgramPipelineId, GL.UseProgramStageMask.FragmentShaderBit, FragmentShader!.Id);
        }

        GL.BindProgramPipeline(ProgramPipelineId);
    }

    public void Dispose()
    {
        ComputeShader?.Dispose();
        VertexShader?.Dispose();
        FragmentShader?.Dispose();
        GL.DeleteProgramPipeline(ProgramPipelineId);
    }

    private Result CreateShaders()
    {
        if (!string.IsNullOrEmpty(_computeShaderSource))
        {
            ComputeShader = new Shader(ShaderType.ComputeShader, _computeShaderSource, "Shader-CS-" + _label);

            return Result.Success();
        }

        if (string.IsNullOrEmpty(_vertexShaderSource))
        {
            return Result.Failure($"File {_vertexShaderSource} does not exist");
        }

        VertexShader = new Shader(ShaderType.VertexShader, _vertexShaderSource, "Shader-VS-" + _label);

        if (string.IsNullOrEmpty(_fragmentShaderSource))
        {
            return Result.Failure($"File {_fragmentShaderSource} does not exist");
        }

        FragmentShader = new Shader(ShaderType.FragmentShader, _fragmentShaderSource, "Shader-FS-" + _label);

        return Result.Success();
    }
}