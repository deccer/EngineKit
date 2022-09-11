using EngineKit.Mathematics;
using Num = System.Numerics;
using Vector4 = EngineKit.Mathematics.Vector4;

namespace EngineKit.Extensions;

public static class Matrix4x4Extensions
{
    public static Matrix ToMatrix(this Num.Matrix4x4 matrix)
    {
        return new Matrix
        {
            Column1 = new Vector4(matrix.M11, matrix.M21, matrix.M31, matrix.M41),
            Column2 = new Vector4(matrix.M12, matrix.M22, matrix.M32, matrix.M42),
            Column3 = new Vector4(matrix.M13, matrix.M23, matrix.M33, matrix.M43),
            Column4 = new Vector4(matrix.M14, matrix.M24, matrix.M34, matrix.M44),
        };
    }
}