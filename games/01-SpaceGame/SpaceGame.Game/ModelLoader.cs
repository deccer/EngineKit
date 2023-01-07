using System.Linq;
using EngineKit.Graphics;

namespace SpaceGame.Game;

public sealed class ModelLoader : IModelLoader
{
    private readonly IMeshLoader _meshLoader;
    private readonly IMaterialLibrary _materialLibrary;

    public ModelLoader(
        IMeshLoader meshLoader,
        IMaterialLibrary materialLibrary)
    {
        _meshLoader = meshLoader;
        _materialLibrary = materialLibrary;
    }

    public Model LoadModel(string name, string filePath)
    {
        var meshDates = _meshLoader.LoadModel(filePath);
        var meshes = meshDates.Select(meshData =>
        {
            var material = _materialLibrary.GetMaterialByName(meshData.MaterialName);
            return new ModelMesh(meshData, material);
        }).ToList();

        return new Model(name, meshes);
    }
}