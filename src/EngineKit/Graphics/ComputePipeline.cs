using System;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

internal sealed class ComputePipeline : Pipeline, IComputePipeline
{
    internal ComputePipeline(ComputePipelineDescriptor computePipelineDescriptor)
    {
        ShaderProgram = new ShaderProgram(
            computePipelineDescriptor.ComputeShaderSource,
            computePipelineDescriptor.PipelineProgramLabel);
    }

    public void Dispatch(uint numGroupX, uint numGroupY, uint numGroupZ)
    {
        GL.Dispatch(numGroupX, numGroupY, numGroupZ);
    }

    public void DispatchIndirect(IIndirectBuffer indirectBuffer, int indirectElementIndex)
    {
        indirectBuffer.Bind();
        GL.DispatchIndirect(new IntPtr(indirectElementIndex * indirectBuffer.Stride));
    }
}