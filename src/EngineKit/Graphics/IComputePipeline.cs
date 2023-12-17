using System.Numerics;

namespace EngineKit.Graphics;

public interface IComputePipeline : IPipeline
{
    void Dispatch(uint numGroupX, uint numGroupY, uint numGroupZ);

    void DispatchIndirect(
        IBuffer dispatchIndirectBuffer,
        int indirectElementIndex);

    void Uniform(int location, float value);
    
    void Uniform(int location, bool transpose, Matrix4x4 value);
}