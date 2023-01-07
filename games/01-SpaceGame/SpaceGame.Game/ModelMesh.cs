using EngineKit.Graphics;

namespace SpaceGame.Game;

public class ModelMesh
{
    public ModelMesh(MeshData meshData, Material material)
    {
        MeshData = meshData;
        Material = material;
    }

    public Material Material { get; set; }

    public MeshData MeshData { get; }
}