using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpaceGame.Game.Ecs;

public class UpdateCameraSystem : ISystem
{
    private readonly EntityWorld _entityWorld;
    private readonly ICamera _camera;

    public UpdateCameraSystem(EntityWorld entityWorld, ICamera camera)
    {
        _entityWorld = entityWorld;
        _camera = camera;
    }

    public void Update(float deltaTime)
    {
        var entities = _entityWorld.GetEntitiesWithComponents<UpdateCameraPositionComponent>();

        var entitiesSpan = CollectionsMarshal.AsSpan(entities);
        ref var entityRef = ref MemoryMarshal.GetReference(entitiesSpan);
        for (var i = 0; i < entitiesSpan.Length; i++)
        {
            var entity = Unsafe.Add(ref entityRef, i);
            var transformComponent = _entityWorld.GetComponent<TransformComponent>(entity.Id);

            _camera.Position = transformComponent.LocalPosition;


        }
    }
}