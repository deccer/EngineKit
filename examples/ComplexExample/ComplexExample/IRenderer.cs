using System;
using System.Numerics;
using EngineKit.Graphics;

namespace ComplexExample;

public interface IRenderer : IDisposable
{
    MeshId AddMeshPrimitive(MeshPrimitive meshPrimitive);

    MaterialId AddMaterial(Material material);
    
    void AddToRenderQueue(MeshId meshIdPrimitive, MaterialId materialId, Matrix4x4 worldMatrix);
    
    void RenderWorld(ICamera camera);

    void ClearRenderQueue();

    void DestroySizeDependentResources();

    void CreateSizeDependentResources();
    bool Load();
    
    void ShowScene();
}