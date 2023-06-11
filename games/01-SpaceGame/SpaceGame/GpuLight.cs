using System.Runtime.InteropServices;
using EngineKit.Mathematics;

namespace SpaceGame;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct GpuLight
{
    public Vector4 PositionAndType; // xyz = position, w = type
    public Vector4 Color; // xyz = color, w = radius
    public Vector4 Attenuation; // x = att.quadratic, y = att.linear, z = att.constant
    public Vector4 DirectionAndCutOff; // xyz = direction, w = cut off

    public static GpuLight CreatePointLight(Vector3 position, Vector3 color, float radius, Vector3 attenuation, float cutOff)
    {
        return new GpuLight
        {
            PositionAndType = new Vector4(position, 0.0f),
            Color = new Vector4(color, radius),
            Attenuation = new Vector4(attenuation, 0.0f),
            DirectionAndCutOff = new Vector4(0.0f, 0.0f, 0.0f, cutOff)
        };
    }

    public static GpuLight CreateSpotLight(Vector3 position, Vector3 color, Vector3 direction, Vector3 attenuation, float cutOffAngle)
    {
        return new GpuLight
        {
            PositionAndType = new Vector4(position, 1.0f),
            Color = new Vector4(color, 1.0f),
            Attenuation = new Vector4(attenuation, 0.0f),
            DirectionAndCutOff = new Vector4(direction, cutOffAngle)
        };
    }

    public static GpuLight CreateDirectionalLight(Vector3 direction, Vector3 color)
    {
        return new GpuLight
        {
            PositionAndType = new Vector4(0.0f, 0.0f, 0.0f, 2.0f),
            Color = new Vector4(color, 0.0f),
            Attenuation = Vector4.Zero,
            DirectionAndCutOff = new Vector4(direction, 0.0f)
        };
    }
}