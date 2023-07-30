namespace EngineKit.Graphics;

public class ModelMesh : IHasName
{
    public ModelMesh(MeshPrimitive meshPrimitive)
    {
        MeshPrimitive = meshPrimitive;
    }
    
    public MeshPrimitive MeshPrimitive { get; }
    
    public string Name => MeshPrimitive.MeshName;
}