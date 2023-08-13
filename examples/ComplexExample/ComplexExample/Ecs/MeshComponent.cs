using EngineKit.Graphics;

namespace ComplexExample.Ecs;

public struct MeshComponent
{
    public MeshComponent(PooledMesh mesh)
    {
        Mesh = mesh;
    }
    
    public PooledMesh Mesh;
}