using EngineKit.Graphics;

namespace DeferredRendering;

public class ModelMesh
{
    public ModelMesh(string name, MeshData meshData)
    {
        Name = name;
        MeshData = meshData;
    }
    
    public string Name { get; }
    
    public MeshData MeshData { get; }
}