using System;
using EngineKit.Mathematics;
using SpaceGame.Game;
using SpaceGame.Game.Ecs;

namespace SpaceGame;

public interface IRenderer : IDisposable
{
    bool Load();

    void Resize();

    void PrepareScene(EntityWorld entityWorld);

    bool RenderScene(ICamera camera, Vector3 directionalLightPosition);

    void RenderShadowDebugUi();
    void CreateShadowMaps(int width, int height);
}