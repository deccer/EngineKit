using System.Runtime.InteropServices;
using Complex.Ecs.Components;
using Complex.Physics;

namespace Complex.Ecs.Systems;

internal class TransformSystem : ITransformSystem
{
    private readonly IEntityWorld _entityWorld;

    private readonly IPhysicsWorld _physicsWorld;

    public TransformSystem(IEntityWorld entityWorld,
                           IPhysicsWorld physicsWorld)
    {
        _entityWorld = entityWorld;
        _physicsWorld = physicsWorld;
    }

    public void Update(float deltaTime)
    {
        var entities = _entityWorld.GetEntitiesWithComponents<PhysicsBodyComponent>();
        var entitiesSpan = CollectionsMarshal.AsSpan(entities);
        for (var i = 0; i < entitiesSpan.Length; i++)
        {
            ref var entity = ref entitiesSpan[i];

            var physicsBody = _entityWorld.GetComponent<PhysicsBodyComponent>(entity.Id);
            if (physicsBody != null)
            {
                entity.LocalMatrix = _physicsWorld.GetBodyPoseByBodyHandle(physicsBody.Handle);
            }
        }
    }
}
