using EngineKit.Graphics;

namespace Complex.Ecs.Components;

public class MaterialComponent : Component
{
    public MaterialComponent(Material material)
    {
        Material = material;
    }

    public Material Material;
    
    public PooledMaterial? MaterialId;

    public Entity Entity;
}