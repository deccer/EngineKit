using OpenTK.Mathematics;

namespace SpaceGame.Game.Ecs.Components;

public class MovableComponent : Component
{
    public Vector3 Velocity;
    
    public Vector3 Acceleration;
}