using EngineKit.Graphics;

namespace ComplexExample.Ecs;

public struct MaterialComponent
{
    public MaterialComponent(MaterialId materialId)
    {
        MaterialId = materialId;
    }
    
    public MaterialId MaterialId;
}