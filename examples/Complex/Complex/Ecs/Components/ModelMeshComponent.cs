using EngineKit.Graphics;

namespace Complex.Ecs.Components;

public class ModelMeshComponent : Component
{
    public ModelMeshComponent(MeshPrimitive meshPrimitive)
    {
        MeshPrimitive = meshPrimitive;
    }
    
    public MeshPrimitive MeshPrimitive;
    
    public PooledMesh? MeshId;

    public Entity Entity;
}