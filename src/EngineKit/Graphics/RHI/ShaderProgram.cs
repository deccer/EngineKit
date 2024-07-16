using System;
using CSharpFunctionalExtensions;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics.RHI;

public sealed class ShaderProgram : IDisposable
{
    private readonly string? _computeShaderSource;
    private readonly string? _vertexShaderSource;
    private readonly string? _fragmentShaderSource;
    private readonly Label _label;

    public uint Id;

    public Shader? VertexShader { get; private set; }

    public Shader? FragmentShader { get; private set; }

    public Shader? ComputeShader { get; private set; }

    public ShaderProgram(
        Label label,
        string computeShaderSource)
    {
        if (string.IsNullOrEmpty(computeShaderSource))
        {
            throw new ArgumentNullException(nameof(computeShaderSource));
        }
        _label = label;
        _computeShaderSource = computeShaderSource;
        _vertexShaderSource = null!;
        _fragmentShaderSource = null!;
    }

    public ShaderProgram(
        Label label,
        string vertexShaderSource,
        string fragmentShaderSource)
    {
        if (string.IsNullOrEmpty(vertexShaderSource))
        {
            throw new ArgumentNullException(nameof(vertexShaderSource));
        }

        if (string.IsNullOrEmpty(fragmentShaderSource))
        {
            throw new ArgumentNullException(nameof(fragmentShaderSource));
        }

        _label = label;
        _computeShaderSource = null!;
        _vertexShaderSource = vertexShaderSource;
        _fragmentShaderSource = fragmentShaderSource;
    }

    public Result Link()
    {
        Id = GL.CreateProgramPipeline();
        GL.ObjectLabel(GL.ObjectIdentifier.ProgramPipeline, Id, $"ProgramPipeline-{_label}");

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
                Id,
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

        GL.UseProgramStages(Id, GL.UseProgramStageMask.VertexShaderBit, VertexShader.Id);
        GL.UseProgramStages(Id, GL.UseProgramStageMask.FragmentShaderBit, FragmentShader.Id);

        return Result.Success();
    }

    public void Use()
    {
        if (ComputeShader != null)
        {
            GL.UseProgramStages(Id, GL.UseProgramStageMask.ComputeShaderBit, ComputeShader.Id);
        }
        else
        {
            GL.UseProgramStages(Id, GL.UseProgramStageMask.VertexShaderBit, VertexShader!.Id);
            GL.UseProgramStages(Id, GL.UseProgramStageMask.FragmentShaderBit, FragmentShader!.Id);
        }

        GL.BindProgramPipeline(Id);
    }

    public void Dispose()
    {
        ComputeShader?.Dispose();
        VertexShader?.Dispose();
        FragmentShader?.Dispose();
        GL.DeleteProgramPipeline(Id);
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

        VertexShader = new Shader(ShaderType.VertexShader, _vertexShaderSource, $"Shader-VS-{_label}");

        if (string.IsNullOrEmpty(_fragmentShaderSource))
        {
            return Result.Failure($"File {_fragmentShaderSource} does not exist");
        }

        FragmentShader = new Shader(ShaderType.FragmentShader, _fragmentShaderSource, $"Shader-FS-{_label}");

        return Result.Success();
    }
}
