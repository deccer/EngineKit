using EngineKit;

namespace Complex.Ecs.Systems;

internal class UpdateCameraSystem : IUpdateCameraSystem
{
    private readonly ICamera _camera;

    private readonly IEntityWorld _entityWorld;

    public UpdateCameraSystem(IEntityWorld entityWorld,
                              ICamera camera)
    {
        _entityWorld = entityWorld;
        _camera = camera;
    }

    public void Update(float deltaTime)
    {
    }
}
