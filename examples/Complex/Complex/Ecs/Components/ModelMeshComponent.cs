using EngineKit.Graphics;

namespace Complex.Ecs.Components;

public class ModelMeshComponent : Component
{
    public PooledMesh? MeshId;

    public MeshPrimitive MeshPrimitive;

    public ModelMeshComponent(MeshPrimitive meshPrimitive)
    {
        MeshPrimitive = meshPrimitive;
    }
}
