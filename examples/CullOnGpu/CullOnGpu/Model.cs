namespace CullOnGpu;

public class Model
{
    public Model(string name, ModelMesh[] modelMeshes)
    {
        Name = name;
        ModelMeshes = modelMeshes;
    }
    
    public string Name { get; set; }

    public ModelMesh[] ModelMeshes { get; }
}