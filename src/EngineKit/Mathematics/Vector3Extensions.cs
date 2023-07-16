namespace EngineKit.Mathematics;

public static class Vector2Extensions
{
    public static Vector2 XX(this Vector2 v) => new(v.X, v.X);

    public static Vector2 YY(this Vector2 v) => new(v.Y, v.Y);

    public static Vector2 YX(this Vector2 v) => new(v.Y, v.X);
}

public static class Vector3Extensions
{
    public static Vector3 XXX(this Vector3 v) => new(v.X, v.X, v.X);

    public static Vector3 XXY(this Vector3 v) => new(v.X, v.X, v.Y);

    public static Vector3 XYY(this Vector3 v) => new(v.X, v.Y, v.Y);

    public static Vector3 XYX(this Vector3 v) => new(v.X, v.Y, v.X);

    public static Vector3 YYY(this Vector3 v) => new(v.Y, v.Y, v.Y);

    public static Vector3 YYX(this Vector3 v) => new(v.Y, v.Y, v.X);

    public static Vector3 YXX(this Vector3 v) => new(v.Y, v.X, v.X);
}

public static class Vector4Extensions
{
    public static Vector3 XYZ(this Vector4 v) => new Vector3(v.X, v.Y, v.Z);
}