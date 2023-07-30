using System.Collections.Generic;

namespace EngineKit.Graphics;

public class Model : IHasName
{
    public Model(string name, IEnumerable<ModelMesh> modelMeshes)
    {
        Name = name;
        ModelMeshes = modelMeshes;
    }
    
    public string Name { get; set; }
    
    public IEnumerable<ModelMesh> ModelMeshes { get; }
}