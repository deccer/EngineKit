using EngineKit.Mathematics;

namespace SpaceGame.Game.Ecs.Components;

public class TransformComponent : Component
{
    public static TransformComponent CreateFromMatrix(Matrix worldMatrix)
    {
        /*
        scale.X = 1.0f / scale.X;
        scale.Y = 1.0f / scale.Y;
        scale.Z = 1.0f / scale.Z;
        */
        worldMatrix.Decompose(out var scale, out var rotation, out var translation);
        return new TransformComponent(translation, rotation, scale);
    }

    public TransformComponent(Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
    {
        LocalPosition = localPosition;
        LocalRotation = localRotation;
        LocalScale = localScale;
    }

    public TransformComponent(Vector3 localPosition, Quaternion localRotation)
    {
        LocalPosition = localPosition;
        LocalRotation = localRotation;
        LocalScale = Vector3.One;
    }

    public TransformComponent(Vector3 localPosition)
    {
        LocalPosition = localPosition;
        LocalRotation = Quaternion.Identity;
        LocalScale = Vector3.One;
    }

    public Vector3 LocalPosition;

    public Quaternion LocalRotation;

    public Vector3 LocalScale;

    public Matrix GlobalWorldMatrix;

    public Matrix GetLocalMatrix()
    {
        return Matrix.Scaling(LocalScale) *
               Matrix.RotationQuaternion(LocalRotation) *
               Matrix.Translation(LocalPosition);
    }
}