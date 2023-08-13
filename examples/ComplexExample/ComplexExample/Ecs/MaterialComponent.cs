using EngineKit.Graphics;

namespace ComplexExample.Ecs;

public struct MaterialComponent
{
    public MaterialComponent(PooledMaterial material)
    {
        Material = material;
    }
    
    public PooledMaterial Material;
}