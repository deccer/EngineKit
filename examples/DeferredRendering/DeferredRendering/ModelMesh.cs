using EngineKit.Graphics;

namespace DeferredRendering;

public class ModelMesh
{
    public ModelMesh(string name, MeshPrimitive meshData)
    {
        Name = name;
        MeshData = meshData;
    }
    
    public string Name { get; }
    
    public MeshPrimitive MeshData { get; }
}