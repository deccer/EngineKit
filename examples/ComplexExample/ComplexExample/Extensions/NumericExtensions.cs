using EngineKit.Mathematics;

namespace ComplexExample.Extensions;

public static class NumericExtensions
{
    public static System.Numerics.Vector2 ToNumVector2(this Vector2 v)
    {
        return new System.Numerics.Vector2(v.X, v.Y);
    }
    
    public static System.Numerics.Vector3 ToNumVector3(this Vector3 v)
    {
        return new System.Numerics.Vector3(v.X, v.Y, v.Z);
    }
    
    public static System.Numerics.Vector4 ToNumVector4(this Vector4 v)
    {
        return new System.Numerics.Vector4(v.X, v.Y, v.Z, v.W);
    }
}