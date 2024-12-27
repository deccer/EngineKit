using System;
using System.Numerics;
using EngineKit;
using EngineKit.Graphics;
using EngineKit.Mathematics;

namespace Complex.Engine;

public interface IRenderer2 : IDisposable
{
    void Clear();

    void AddMeshInstance(MeshPrimitive meshPrimitive,
                         Material material,
                         Matrix4x4 transform,
                         BoundingBox transformedMeshAabb);

    void Render(ICamera camera);

    bool Load();

    void RenderUI();

    FramebufferDescriptor GetMainFramebufferDescriptor();
}
