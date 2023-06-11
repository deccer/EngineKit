using EngineKit.Mathematics;

namespace SpaceGame.Game.Lights;

public class PointLight : Light
{
    public float Radius { get; set; }

    public PointLight(
        Vector3 color,
        Vector3 attenuation,
        float radius)
        : base(color, attenuation)
    {
        Radius = radius;
    }
}