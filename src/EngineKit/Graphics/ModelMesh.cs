namespace EngineKit.Graphics;

public class ModelMesh
{
    public string Name { get; }

    public MeshData MeshData { get; }

    public Material Material { get; }

    public ModelMesh(string name, MeshData meshData)
    {
        Name = name;
        MeshData = meshData;
    }
}