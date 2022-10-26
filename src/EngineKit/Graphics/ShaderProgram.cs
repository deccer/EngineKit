using System;
using System.IO;
using System.Reflection.Emit;
using CSharpFunctionalExtensions;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

public class ShaderProgram : IDisposable
{
    private readonly string? _computeShaderFilePath;
    private readonly string? _vertexShaderFilePath;
    private readonly string? _fragmentShaderFilePath;
    private readonly Label _label;

    public uint ProgramPipelineId;

    public Shader? VertexShader { get; private set; }

    public Shader? FragmentShader { get; private set; }

    public Shader? ComputeShader { get; private set; }

    public ShaderProgram(string computeShaderFilePath, Label label)
    {
        _computeShaderFilePath = computeShaderFilePath;
        _label = label;
        _vertexShaderFilePath = null!;
        _fragmentShaderFilePath = null!;
    }

    public ShaderProgram(
        string vertexShaderFilePath,
        string fragmentShaderFilePath,
        Label label)
    {
        _computeShaderFilePath = null!;
        _vertexShaderFilePath = vertexShaderFilePath;
        _fragmentShaderFilePath = fragmentShaderFilePath;
        _label = label;
    }

    public Result Link()
    {
        ProgramPipelineId = GL.CreateProgramPipeline();
        GL.ObjectLabel(GL.ObjectIdentifier.ProgramPipeline, ProgramPipelineId, _label);

        var compilationResult = Compile();
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
                GL.UseProgramStageMask.VertexShaderBit,
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

    private Result Compile()
    {
        if (_computeShaderFilePath != null)
        {
            if (!File.Exists(_computeShaderFilePath))
            {
                return Result.Failure($"File {_computeShaderFilePath} does not exist");
            }

            var computeShaderSource = File.ReadAllText(_computeShaderFilePath);
            ComputeShader = new Shader(ShaderType.ComputeShader, computeShaderSource, "CS" + _label);

            return Result.Success();
        }

        if (!File.Exists(_vertexShaderFilePath))
        {
            return Result.Failure($"File {_vertexShaderFilePath} does not exist");
        }

        var vertexShaderSource = File.ReadAllText(_vertexShaderFilePath);
        VertexShader = new Shader(ShaderType.VertexShader, vertexShaderSource, "VS" + _label);

        if (!File.Exists(_fragmentShaderFilePath))
        {
            return Result.Failure($"File {_fragmentShaderFilePath} does not exist");
        }

        var fragmentShaderSource = File.ReadAllText(_fragmentShaderFilePath);

        FragmentShader = new Shader(ShaderType.FragmentShader, fragmentShaderSource, "FS" + _label);

        return Result.Success();
    }
}