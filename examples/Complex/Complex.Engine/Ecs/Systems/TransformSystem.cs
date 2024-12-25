using System.Runtime.InteropServices;
using Complex.Engine.Ecs.Components;
using Complex.Engine.Physics;

namespace Complex.Engine.Ecs.Systems;

public class TransformSystem : ITransformSystem
{
    private readonly IEntityRegistry _entityRegistry;

    private readonly IPhysicsWorld _physicsWorld;

    public TransformSystem(IEntityRegistry entityRegistry,
                           IPhysicsWorld physicsWorld)
    {
        _entityRegistry = entityRegistry;
        _physicsWorld = physicsWorld;
    }

    public void Update(float deltaTime)
    {
        var entities = _entityRegistry.GetEntitiesWithComponents<PhysicsBodyComponent>();
        var entitiesSpan = CollectionsMarshal.AsSpan(entities);
        for (var i = 0; i < entitiesSpan.Length; i++)
        {
            ref var entity = ref entitiesSpan[i];

            var physicsBody = _entityRegistry.GetComponent<PhysicsBodyComponent>(entity.Id);
            if (physicsBody != null)
            {
                entity.LocalMatrix = _physicsWorld.GetBodyPoseByBodyHandle(physicsBody.Handle);
            }
        }
    }
}
