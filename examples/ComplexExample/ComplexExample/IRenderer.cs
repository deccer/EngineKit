using System;
using System.Numerics;
using EngineKit.Graphics;

namespace ComplexExample;

public interface IRenderer : IDisposable
{
    PooledMesh AddMeshPrimitive(MeshPrimitive meshPrimitive);

    PooledMaterial AddMaterial(Material material);
    
    void AddToRenderQueue(PooledMesh meshPrimitive, PooledMaterial material, Matrix4x4 worldMatrix);
    
    void RenderWorld(ICamera camera);

    void ClearRenderQueue();

    void DestroySizeDependentResources();

    void CreateSizeDependentResources();
    bool Load();
    
    void ShowScene();
}