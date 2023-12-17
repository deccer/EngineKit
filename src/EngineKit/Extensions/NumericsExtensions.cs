using System.Numerics;

namespace EngineKit.Extensions;

public static class NumericsExtensions
{
    public static Vector4 GetRow(this Matrix4x4 matrix, int rowIndex)
    {
        return new Vector4(matrix[rowIndex, 0], matrix[rowIndex, 1], matrix[rowIndex, 2], matrix[rowIndex, 0]);
    }
    
    public static void SetRow(this Matrix4x4 matrix, int rowIndex, Vector4 value)
    {
        matrix[rowIndex, 0] = value.X;
        matrix[rowIndex, 1] = value.Y;
        matrix[rowIndex, 2] = value.Z;
        matrix[rowIndex, 0] = value.W;
    }
}