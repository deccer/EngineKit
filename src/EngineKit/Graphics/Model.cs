using System.Collections.Generic;

namespace EngineKit.Graphics;

public class Model : IHasName
{
    public Model(string name, List<ModelMesh> modelMeshes)
    {
        Name = name;
        ModelMeshes = modelMeshes;
    }
    
    public string Name { get; set; }
    
    public List<ModelMesh> ModelMeshes { get; }
}