namespace EngineKit.Graphics.Assets;

using Serilog;
using SharpGLTF.Schema2;
using SharpGLTF.Validation;
using System;
using System.Collections.Generic;
using System.IO;
using MeshPrimitive = EngineKit.Graphics.MeshPrimitive;

internal sealed class SharpGltfMeshLoader2 : IMeshLoader
{
    private readonly ILogger _logger;

    public SharpGltfMeshLoader2(ILogger logger)
    {
        _logger = logger;
    }

    public IReadOnlyCollection<MeshPrimitive> LoadMeshPrimitivesFromFile(string filePath)
    {
        if(!File.Exists(filePath))
        {
            _logger.Error($"File {filePath} does not exist");

            return Array.Empty<MeshPrimitive>();
        }

        var readSettings = new ReadSettings { Validation = ValidationMode.Skip, };

        var model = ModelRoot.Load(filePath, readSettings);

        return new List<MeshPrimitive>();
    }
}
