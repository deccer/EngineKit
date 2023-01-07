using JoltPhysicsSharp;

namespace SpaceGame.Game.Ecs;

public class PhysicsBodyComponent : Component
{
    public PhysicsBodyComponent(Body rigidBody)
    {
        RigidBody = rigidBody;
    }

    public Body RigidBody { get; }
}