using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SpaceGame.Game.Ecs.Components;
using SpaceGame.Game.Physics;

namespace SpaceGame.Game.Ecs.Systems;

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
        var entities = _entityWorld
            .GetEntitiesWithComponents<TransformComponent>()
            .OrderByDescending(e => e.Level)
            .ToList();

        var entitiesSpan = CollectionsMarshal.AsSpan(entities);
        ref var entityRef = ref MemoryMarshal.GetReference(entitiesSpan);
        for (var i = 0; i < entitiesSpan.Length; i++)
        {
            var entity = Unsafe.Add(ref entityRef, i);

            var transformComponent = _entityWorld.GetComponent<TransformComponent>(entity.Id);
            if (transformComponent == null)
            {
                continue;
            }

            var physicsBodyComponent = _entityWorld.GetComponent<PhysicsBodyComponent>(entity.Id);
            if (physicsBodyComponent != null)
            {
                transformComponent.LocalPosition = _physicsWorld.GetPosition(physicsBodyComponent.RigidBody.ID);
            }

            var parentComponent = _entityWorld.GetComponent<ParentComponent>(entity.Id);
            if (parentComponent != null && parentComponent.Entity != transformComponent.Entity)
            {
                var parentTransformComponent = _entityWorld.GetComponent<TransformComponent>(parentComponent.Parent);
                if (parentTransformComponent != null)
                {
                    transformComponent.GlobalWorldMatrix = parentTransformComponent.GetLocalMatrix() * transformComponent.GetLocalMatrix();
                }
                else
                {
                    transformComponent.GlobalWorldMatrix = transformComponent.GetLocalMatrix();
                }
            }
            else
            {
                transformComponent.GlobalWorldMatrix = transformComponent.GetLocalMatrix();
            }
        }
    }
}