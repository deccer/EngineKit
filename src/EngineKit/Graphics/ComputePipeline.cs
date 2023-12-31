using System.Numerics;
using EngineKit.Extensions;
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

    public unsafe void DispatchIndirect(IBuffer dispatchIndirectBuffer, int indirectElementIndex)
    {
        GL.BindBuffer(BufferTarget.DispatchIndirectBuffer.ToGL(), dispatchIndirectBuffer.Id);
        GL.DispatchIndirect(new nint(indirectElementIndex * sizeof(GpuIndirectDispatchData)));
    }

    public void Uniform(int location, float value)
    {
        GL.ProgramUniform(ShaderProgram!.ComputeShader!.Id, location, value);
    }
    
    public void Uniform(int location, bool transpose, Matrix4x4 value)
    {
        GL.ProgramUniform(ShaderProgram!.ComputeShader!.Id, location, transpose, value);
    }
}