using System.Collections.Generic;
using System.IO;
using System.Linq;
using EngineKit.Graphics;
using Serilog;

namespace SpaceGame.Game;

public sealed class ModelLibrary : IModelLibrary
{
    private readonly ILogger _logger;

    private readonly IMeshLoader _meshLoader;

    private readonly IMaterialLibrary _materialLibrary;

    public ModelLibrary(
        ILogger logger,
        IMeshLoader meshLoader,
        IMaterialLibrary materialLibrary)
    {
        _logger = logger.ForContext<ModelLibrary>();
        _meshLoader = meshLoader;
        _materialLibrary = materialLibrary;
        Models = new List<Model>(256);
    }

    public IList<Model> Models { get; }

    public void AddModel(string name, string filePath)
    {
        if (!File.Exists(filePath))
        {
            _logger.Error("{Category}: Unable to load model '{Name}' from {FilePath}", nameof(ModelLibrary), name, filePath);
            return;
        }

        var meshDates = _meshLoader.LoadModel(filePath);
        var meshes = meshDates.Select(meshData =>
        {
            var material = string.IsNullOrEmpty(meshData.MaterialName)
                    ? _materialLibrary.GetMaterialByName("M_Default")
                    : _materialLibrary.GetMaterialByName(meshData.MaterialName);
            return new ModelMesh(meshData, material);
        }).ToList();

        var model = new Model(name, meshes);
        Models.Add(model);
    }

    public ModelMesh? GetMeshDataByName(string meshDataName)
    {
        return Models
            .SelectMany(m => m.Meshes)
            .FirstOrDefault(m => m.MeshData.MeshName == meshDataName);
    }
}