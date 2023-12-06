using System;
using EngineKit.Graphics.Shaders;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

internal sealed class ComputePipeline : Pipeline, IComputePipeline
{
    private readonly ComputePipelineDescriptor _computePipelineDescriptor;

    internal ComputePipeline(ComputePipelineDescriptor computePipelineDescriptor, ShaderProgram shaderProgram)
    {
        _computePipelineDescriptor = computePipelineDescriptor;
        ShaderProgram = shaderProgram;
        Label = computePipelineDescriptor.PipelineProgramLabel;
    }

    public void Dispatch(uint numGroupX, uint numGroupY, uint numGroupZ)
    {
        GL.Dispatch(numGroupX, numGroupY, numGroupZ);
    }

    public void DispatchIndirect(IIndirectBuffer indirectBuffer, int indirectElementIndex)
    {
        indirectBuffer.Bind();
        GL.DispatchIndirect(new nint(indirectElementIndex * indirectBuffer.Stride));
    }

    public void Uniform(int location, float value)
    {
        GL.ProgramUniform(ShaderProgram.ComputeShader.Id, location, value);
    }
}