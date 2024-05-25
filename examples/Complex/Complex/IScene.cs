using System;
using System.Numerics;
using Complex.Ecs;
using EngineKit.Graphics;

namespace Complex;

public interface IScene : IDisposable
{
    void AddEntityWithModelRenderer(string name,
                                    EntityId? parent,
                                    Model model,
                                    Matrix4x4 startWorldMatrix);

    void AddEntityWithModelMeshRenderer(string name,
        EntityId? parent,
        ModelMesh modelMesh,
        Matrix4x4 startWorldMatrix);

    EntityId GetRoot();

    void Update(float deltaTime);
}
