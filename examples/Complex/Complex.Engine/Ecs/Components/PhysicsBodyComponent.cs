using BepuPhysics;

namespace Complex.Engine.Ecs.Components;

public class PhysicsBodyComponent : Component
{
    public PhysicsBodyComponent(BodyHandle handle)
    {
        Handle = handle;
    }

    public BodyHandle Handle { get; }
}
