using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using CSharpFunctionalExtensions;
using EngineKit.Graphics;

namespace Complex;

internal class AssetLoader : IAssetLoader
{
    private readonly IMeshLoader _meshLoader;
    private readonly IGraphicsContext _graphicsContext;
    private readonly IMaterialLibrary _materialLibrary;

    private readonly IDictionary<string, IList<string>> _assetNameToMeshPrimitiveNames;
    private readonly IDictionary<string, (PooledMesh MeshId, Matrix4x4 Transform)> _meshPrimitiveNameToMeshId;
    private readonly IDictionary<string, PooledMaterial> _meshPrimitiveNameToMaterialId;

    private readonly Lazy<IMeshPool> _meshPool;
    private readonly Lazy<IMaterialPool> _materialPool;

    public AssetLoader(
        IMeshLoader meshLoader,
        IGraphicsContext graphicsContext,
        IMaterialLibrary materialLibrary,
        ISamplerLibrary samplerLibrary)
    {
        _meshLoader = meshLoader;
        _graphicsContext = graphicsContext;
        _materialLibrary = materialLibrary;
        _meshPool = new Lazy<IMeshPool>(() => _graphicsContext.CreateMeshPool("ModelVertices", 512 * 1024 * 1024, 768 * 1024 * 1024));
        _materialPool = new Lazy<IMaterialPool>(() => _graphicsContext.CreateMaterialPool("ModelMaterials", 1024 * 1024, samplerLibrary));
        _assetNameToMeshPrimitiveNames = new Dictionary<string, IList<string>>();
        _meshPrimitiveNameToMeshId = new Dictionary<string, (PooledMesh MeshId, Matrix4x4 Transform)>();
        _meshPrimitiveNameToMaterialId = new Dictionary<string, PooledMaterial>();
    }

    public IBuffer GetVertexBuffer()
    {
        return _meshPool.Value.VertexBuffer;
    }

    public IBuffer GetIndexBuffer()
    {
        return _meshPool.Value.IndexBuffer;
    }

    public IReadOnlyList<string> GetAssetNames()
    {
        return _assetNameToMeshPrimitiveNames.Keys.ToList();
    }

    public IReadOnlyList<string> GetMeshPrimitiveNamesByAssetName(string assetName)
    {
        if (_assetNameToMeshPrimitiveNames.TryGetValue(assetName, out var meshPrimitiveNames))
        {
            return meshPrimitiveNames.ToList();
        }
        
        return ImmutableList<string>.Empty;
    }
    
    public void ImportAsset(string name, string filePath)
    {
        if (_assetNameToMeshPrimitiveNames.TryGetValue(name, out var meshPrimitiveNames))
        {
            return;
        }

        meshPrimitiveNames = new List<string>();
        var meshPrimitives = _meshLoader.LoadMeshPrimitivesFromFile(filePath);
        foreach (var meshPrimitive in meshPrimitives)
        {
            var meshPrimitiveName = string.IsNullOrEmpty(meshPrimitive.MeshName)
                ? Guid.NewGuid().ToString()
                : meshPrimitive.MeshName;
            var meshId = _meshPool.Value.GetOrAdd(meshPrimitive);
            if (!meshPrimitiveNames.Contains(meshPrimitiveName))
            {
                meshPrimitiveNames.Add(meshPrimitiveName);
                _meshPrimitiveNameToMeshId.Add(meshPrimitiveName, (meshId, meshPrimitive.Transform));
            }
            
            if (!string.IsNullOrEmpty(meshPrimitive.MaterialName))
            {
                if (!_meshPrimitiveNameToMaterialId.ContainsKey(meshPrimitiveName))
                {
                    var material = _materialLibrary.GetMaterialByName(meshPrimitive.MaterialName);
                    var materialId = _materialPool.Value.GetOrAdd(material);
                    _meshPrimitiveNameToMaterialId.Add(meshPrimitiveName, materialId);
                }
            }
        }

        _assetNameToMeshPrimitiveNames.Add(name, meshPrimitiveNames);
    }

    public Maybe<(PooledMesh MeshId, Matrix4x4 Transform)> GetMeshIdByMeshPrimitiveName(string meshPrimitiveName)
    {
        return _meshPrimitiveNameToMeshId.TryGetValue(meshPrimitiveName, out var meshIdAndTransform) 
            ? (meshIdAndTransform.MeshId, meshIdAndTransform.Transform)
            : Maybe<(PooledMesh, Matrix4x4)>.None;
    }
    
    public Maybe<PooledMaterial> GetMaterialIdByMeshPrimitiveName(string meshPrimitiveName)
    {
        return _meshPrimitiveNameToMaterialId.TryGetValue(meshPrimitiveName, out var meshId) 
            ? meshId
            : Maybe<PooledMaterial>.None;
    }
}