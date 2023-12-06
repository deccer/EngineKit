using System.Numerics;

namespace ComplexExample.Ecs;

public struct TransformComponent
{
    public TransformComponent(Matrix4x4 matrix)
    {
        LocalMatrix = matrix;
        Position =  new Vector3(matrix.M41, matrix.M42, matrix.M43);
        Rotation = Quaternion.CreateFromRotationMatrix(matrix);
        Scale = new Vector3(matrix.M11, matrix.M22, matrix.M33);
    }
    
    public Vector3 Position;

    public Quaternion Rotation;

    public Vector3 Scale;

    public Matrix4x4 GlobalMatrix;

    public Matrix4x4 LocalMatrix;
}