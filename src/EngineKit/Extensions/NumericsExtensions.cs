using System.Numerics;

namespace EngineKit.Extensions;

public static class NumericsExtensions
{
    public static Vector2 ToVector2(this System.Numerics.Vector2 v)
    {
        return new Vector2(v.X, v.Y);
    }
    
    public static Vector3 ToVector3(this System.Numerics.Vector3 v)
    {
        return new Vector3(v.X, v.Y, v.Z);
    }
    
    public static Vector4 ToVector4(this System.Numerics.Vector4 v)
    {
        return new Vector4(v.X, v.Y, v.Z, v.W);
    }
}