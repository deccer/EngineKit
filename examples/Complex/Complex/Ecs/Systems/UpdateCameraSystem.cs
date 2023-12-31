using EngineKit;

namespace Complex.Ecs.Systems;

internal class UpdateCameraSystem : IUpdateCameraSystem
{
    private readonly IEntityWorld _entityWorld;
    private readonly ICamera _camera;

    public UpdateCameraSystem(IEntityWorld entityWorld, ICamera camera)
    {
        _entityWorld = entityWorld;
        _camera = camera;
    }
    
    public void Update(float deltaTime)
    {
        
    }
}