namespace EngineKit.Graphics;

public interface IComputePipeline : IPipeline
{
    void Dispatch(uint numGroupX, uint numGroupY, uint numGroupZ);

    void DispatchIndirect(
        IIndirectBuffer indirectBuffer,
        int indirectElementIndex);
}