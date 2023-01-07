using System;
using OpenTK.Mathematics;

namespace SpaceGame.Game.Lights;

public abstract class Light
{
    public Guid Id { get; }

    public Vector3 Color { get; set; }

    public Vector3 Attenuation { get; set; }

    protected Light(Vector3 color, Vector3 attenuation)
    {
        Color = color;
        Attenuation = attenuation;
    }

    public static void CreatePointLight(Vector3 color, float radius, Vector3 attenuation)
    {
        //parent.AddComponent(new PointLight(color, attenuation, radius));
    }

    public static void CreateSpotLight(Vector3 color, Vector3 direction, Vector3 attenuation, float cutOffAngle)
    {
        //parent.AddComponent(new SpotLight(color, attenuation, direction, cutOffAngle));
    }
}