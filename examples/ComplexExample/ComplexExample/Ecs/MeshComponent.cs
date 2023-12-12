using EngineKit.Graphics;

namespace ComplexExample.Ecs;

public struct MeshComponent
{
    public MeshComponent(MeshId meshId)
    {
        MeshId = meshId;
    }
    
    public MeshId MeshId;
}