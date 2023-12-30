using BepuPhysics;

namespace Complex.Ecs.Components;

public class PhysicsBodyComponent : Component
{
    public BodyHandle Handle { get; }

    public PhysicsBodyComponent(BodyHandle handle)
    {
        Handle = handle;
    }
}