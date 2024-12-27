using EngineKit;

namespace Complex.Engine.Ecs.Systems;

public class UpdateCameraSystem : IUpdateCameraSystem
{
    private readonly ICamera _camera;

    private readonly IEntityRegistry _entityRegistry;

    public UpdateCameraSystem(IEntityRegistry entityRegistry,
                              ICamera camera)
    {
        _entityRegistry = entityRegistry;
        _camera = camera;
    }

    public void Update(float deltaTime)
    {
    }
}
