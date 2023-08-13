using EngineKit.Mathematics;

namespace ComplexExample.Ecs;

public struct TransformComponent
{
    public TransformComponent(Matrix matrix)
    {
        LocalMatrix = matrix;
        Position = matrix.TranslationVector;
        Rotation = Quaternion.RotationMatrix(matrix);
        Scale = matrix.ScaleVector;
    }
    
    public Vector3 Position;

    public Quaternion Rotation;

    public Vector3 Scale;

    public Matrix GlobalMatrix;

    public Matrix LocalMatrix;
}