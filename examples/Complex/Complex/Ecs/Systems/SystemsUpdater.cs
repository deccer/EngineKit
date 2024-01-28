namespace Complex.Ecs.Systems;

internal class SystemsUpdater : ISystemsUpdater
{
    private readonly IPreRenderSystem _preRenderSystem;

    private readonly ITransformSystem _transformSystem;

    private readonly IUpdateCameraSystem _updateCameraSystem;

    public SystemsUpdater(IUpdateCameraSystem updateCameraSystem,
                          ITransformSystem transformSystem,
                          IPreRenderSystem preRenderSystem)
    {
        _updateCameraSystem = updateCameraSystem;
        _transformSystem = transformSystem;
        _preRenderSystem = preRenderSystem;
    }

    public void Update(float deltaTime)
    {
        _updateCameraSystem.Update(deltaTime);
        _transformSystem.Update(deltaTime);
        _preRenderSystem.Update();
    }
}
