using EngineKit.Mathematics;

namespace SpaceGame.Game.Lights;

public class SpotLight : Light
{
    public Vector3 Direction { get; set; }

    public float CutOffAngle { get; set; }

    public SpotLight(
        Vector3 color,
        Vector3 attenuation,
        Vector3 direction,
        float cutOffAngle) : base(color, attenuation)
    {
        Direction = direction;
        CutOffAngle = cutOffAngle;
    }
}