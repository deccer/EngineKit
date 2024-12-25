using EngineKit.Graphics;

namespace Complex.Engine.Ecs.Components;

public class ModelMeshComponent : Component
{
    public PooledMesh? MeshId;

    public MeshPrimitive MeshPrimitive;

    public ModelMeshComponent(MeshPrimitive meshPrimitive)
    {
        MeshPrimitive = meshPrimitive;
    }
}
