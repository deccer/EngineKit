using BepuPhysics;

namespace Complex.Ecs.Components;

public class PhysicsBodyComponent : Component
{
    public PhysicsBodyComponent(BodyHandle handle)
    {
        Handle = handle;
    }

    public BodyHandle Handle { get; }
}
