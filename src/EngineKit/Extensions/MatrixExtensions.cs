using OpenTK.Mathematics;
using Num = System.Numerics;

namespace EngineKit.Extensions;

public static class Matrix4x4Extensions
{
    public static Matrix4 ToMatrix(this Num.Matrix4x4 matrix)
    {
        return new Matrix4(
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44);
    }
}