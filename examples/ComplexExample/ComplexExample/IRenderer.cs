using System;
using EngineKit.Graphics;
using EngineKit.Mathematics;

namespace ComplexExample;

public interface IRenderer : IDisposable
{
    PooledMesh AddMeshPrimitive(MeshPrimitive meshPrimitive);

    PooledMaterial AddMaterial(Material material);
    
    void AddToRenderQueue(PooledMesh meshPrimitive, PooledMaterial material, Matrix worldMatrix);
    
    void RenderWorld(ICamera camera);

    void ClearRenderQueue();

    void DestroySizeDependentResources();

    void CreateSizeDependentResources();
    bool Load();
    
    void ShowScene();
}