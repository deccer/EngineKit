namespace EngineKit.Graphics;

public readonly struct PooledMaterial
{
    public PooledMaterial(uint index)
    {
        Index = index;
    }

    public readonly uint Index;
}