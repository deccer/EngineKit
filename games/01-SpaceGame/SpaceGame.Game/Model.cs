using System.Collections.Generic;

namespace SpaceGame.Game;

public class Model
{
    public Model(string name, ICollection<ModelMesh> meshes)
    {
        Name = name;
        Meshes = meshes;
    }

    public string Name { get; }

    public ICollection<ModelMesh> Meshes { get; }
}