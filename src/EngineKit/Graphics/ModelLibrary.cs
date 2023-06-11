using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EngineKit.Graphics;

internal sealed class ModelLibrary : IModelLibrary
{
    private readonly IMeshLoader _meshLoader;
    private readonly string _baseDirectory;
    private readonly IDictionary<string, Model> _models;

    public ModelLibrary(IMeshLoader meshLoader)
    {
        _meshLoader = meshLoader;
        _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        _models = new Dictionary<string, Model>(64);
    }

    public void AddModel(Model model)
    {
        if (_models.ContainsKey(model.Name))
        {
            return;
        }
        
        _models.Add(model.Name, model);
    }

    public void AddModelFromFile(string name, string filePath)
    {
        var meshPrimitives = _meshLoader.LoadMeshPrimitivesFromFile(Path.Combine(_baseDirectory, filePath));
        var modelMeshes = meshPrimitives.Select(meshPrimitive => new ModelMesh(meshPrimitive));
        _models.Add(name, new Model(name, modelMeshes));
    }

    public Model? GetModelByName(string name)
    {
        return _models.TryGetValue(name, out var model)
            ? model
            : null;
    }

    public IReadOnlyCollection<string> GetModelNames()
    {
        return _models.Keys.ToArray();
    }
}