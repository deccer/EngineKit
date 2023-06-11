namespace EngineKit.Graphics;

public readonly struct PooledMaterial
{
    public PooledMaterial(int index)
    {
        Index = index;
    }

    public readonly int Index;
}