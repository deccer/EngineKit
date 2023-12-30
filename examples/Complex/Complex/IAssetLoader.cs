using System.Collections.Generic;
using System.Numerics;
using CSharpFunctionalExtensions;
using EngineKit.Graphics;

namespace Complex;

internal interface IAssetLoader
{
    void ImportAsset(string name, string filePath);

    IReadOnlyList<string> GetAssetNames();
    
    Maybe<(PooledMesh MeshId, Matrix4x4 Transform)> GetMeshIdByMeshPrimitiveName(string meshPrimitiveName);
    
    Maybe<PooledMaterial> GetMaterialIdByMeshPrimitiveName(string meshPrimitiveName);
    
    IReadOnlyList<string> GetMeshPrimitiveNamesByAssetName(string assetName);
    
    IBuffer GetVertexBuffer();
    
    IBuffer GetIndexBuffer();
}