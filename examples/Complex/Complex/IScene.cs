using System;
using System.Numerics;
using Arch.Core;
using EngineKit.Graphics;

namespace Complex;

internal interface IScene : IDisposable
{
    void AddEntityWithModelRenderer(Entity? parent, Model model, Vector3 startPosition);

    Entity GetRoot();
    
    void Update(float deltaTime);
}