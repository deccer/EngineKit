using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SpaceGame.Game.Physics;

namespace SpaceGame.Game.Ecs;

public class TransformSystem : ISystem
{
    private readonly EntityWorld _entityWorld;
    private readonly IPhysicsWorld _physicsWorld;

    public TransformSystem(EntityWorld entityWorld, IPhysicsWorld physicsWorld)
    {
        _entityWorld = entityWorld;
        _physicsWorld = physicsWorld;

    }
    public void Update(float deltaTime)
    {
        var entities = _entityWorld.GetEntitiesWithComponents<TransformComponent>();

        var entitiesSpan = CollectionsMarshal.AsSpan(entities);
        ref var entityRef = ref MemoryMarshal.GetReference(entitiesSpan);
        for (var i = 0; i < entitiesSpan.Length; i++)
        {
            var entity = Unsafe.Add(ref entityRef, i);

            var physicsBody = _entityWorld.GetComponent<PhysicsBodyComponent>(entity.Id);
            if (physicsBody != null)
            {
                entity.UpdateTransforms(_physicsWorld.GetPosition(physicsBody.RigidBody.ID));
            }
            else
            {
                entity.UpdateTransforms();
            }

            entity.UpdateTransforms();
        }
    }
}