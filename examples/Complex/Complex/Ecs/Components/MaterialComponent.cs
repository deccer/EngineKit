using EngineKit.Graphics;

namespace Complex.Ecs.Components;

public class MaterialComponent : Component
{
    public Material Material;

    public PooledMaterial? MaterialId;

    public MaterialComponent(Material material)
    {
        Material = material;
    }
}
