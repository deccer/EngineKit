using SpaceGame.Game.Ecs.Components;

namespace SpaceGame.Game.Ecs.Systems;

public class MovementSystem : ISystem
{
    private readonly EntityWorld _entityWorld;

    public MovementSystem(EntityWorld entityWorld)
    {
        _entityWorld = entityWorld;

    }
    public void Update(float deltaTime)
    {
        var entities = _entityWorld.GetEntitiesWithComponents<TransformComponent, MovableComponent>();

        foreach (var entity in entities)
        {
            var transform = _entityWorld.GetComponent<TransformComponent>(entity.Id);
            var movable = _entityWorld.GetComponent<MovableComponent>(entity.Id);
            if (movable == null)
            {
                break;
            }
            transform.LocalPosition += movable.Velocity * deltaTime + 0.5f * movable.Acceleration * deltaTime * deltaTime;
            movable.Velocity += movable.Acceleration * deltaTime;
        }
    }
}