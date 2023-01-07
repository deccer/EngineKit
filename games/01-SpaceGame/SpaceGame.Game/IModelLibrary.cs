using System.Collections.Generic;

namespace SpaceGame.Game;

public interface IModelLibrary
{
    IList<Model> Models { get; }

    void AddModel(string name, string filePath);

    ModelMesh? GetMeshDataByName(string meshDataName);
}