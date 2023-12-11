using EngineKit.Graphics;

namespace CullOnGpu;

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