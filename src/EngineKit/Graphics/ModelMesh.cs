namespace EngineKit.Graphics;

public class ModelMesh
{
    public ModelMesh(MeshPrimitive meshPrimitive)
    {
        MeshPrimitive = meshPrimitive;
    }
    
    public MeshPrimitive MeshPrimitive { get; }
}