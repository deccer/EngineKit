using EngineKit.Mathematics;

namespace SpaceGame.Game.Lights;

public class DirectionalLight : Light
{
    public Vector3 Direction { get; set; }

    public DirectionalLight(

        Vector3 color, Vector3 direction)
        : base(color, Vector3.Zero)
    {
        Direction = direction;
    }
}