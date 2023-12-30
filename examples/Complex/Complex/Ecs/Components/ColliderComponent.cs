using BepuPhysics.Collidables;

namespace Complex.Ecs.Components;

public class ColliderComponent : Component
{
    public Collidable Collidable;
    public Entity Entity { get; set; }
}