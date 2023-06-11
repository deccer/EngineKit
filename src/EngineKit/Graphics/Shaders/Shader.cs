using System;
using CSharpFunctionalExtensions;
using EngineKit.Extensions;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics.Shaders;

public sealed class Shader : IDisposable
{
    private readonly ShaderType _shaderType;
    private readonly string _shaderSource;
    private readonly string _label;

    public Shader(ShaderType shaderType, string shaderSource, string label = "")
    {
        _shaderType = shaderType;
        _shaderSource = shaderSource;
        _label = label;
    }

    public uint Id { get; private set; }

    public void Dispose()
    {
        if (Id > 0)
        {
            GL.DeleteProgram(Id);
        }
    }

    internal Result Compile()
    {
        Id = GL.CreateShaderProgram(_shaderType.ToGL(), _shaderSource);
        GL.ObjectLabel(GL.ObjectIdentifier.Program, Id, _label);
        GL.ProgramParameter(Id, GL.ProgramParameterType.ProgramSeparable, 1);

        var linkStatus = GL.GetProgram(Id, GL.ProgramProperty.LinkStatus);
        if (linkStatus == 0)
        {
            var infoLogLength = 0;
            var errorMessage = GL.GetProgramInfoLog(Id, 1024, ref infoLogLength);
            GL.DeleteProgram(Id);

            GL.DebugMessageInsert(
                GL.DebugSource.Application,
                GL.DebugType.Error,
                1000,
                GL.DebugSeverity.High,
                errorMessage);
            return Result.Failure($"{_label}: {errorMessage}");
        }

        return Result.Success();
    }
}