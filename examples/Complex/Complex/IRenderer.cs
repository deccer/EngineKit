using System;
using System.Numerics;
using EngineKit.Graphics;
using EngineKit.Mathematics;

namespace Complex;

public interface IRenderer : IDisposable
{
    void Clear();
    
    void AddMeshInstance(
        MeshPrimitive meshPrimitive,
        Material material,
        Matrix4x4 transform,
        BoundingBox transformedMeshAabb);
    
    void Render(ICamera camera);
    
    void ResizeFramebufferDependentResources();
    
    bool Load();
    void RenderUI();
}