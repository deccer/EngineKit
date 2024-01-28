using System.Numerics;

namespace Complex.Ecs.Components;

public class XTransformComponent : Component
{
    public Matrix4x4 GlobalWorldMatrix;

    public bool IsSrt;

    public Vector3 LocalPosition = Vector3.Zero;

    public Quaternion LocalRotation = Quaternion.Identity;

    public Vector3 LocalScale = Vector3.One;

    public Matrix4x4 LocalWorldMatrix;

    public static XTransformComponent CreateFromMatrix(Matrix4x4 worldMatrix)
    {
        /*
        scale.X = 1.0f / scale.X;
        scale.Y = 1.0f / scale.Y;
        scale.Z = 1.0f / scale.Z;
        */
        Matrix4x4.Decompose(worldMatrix,
                out var scale,
                out var rotation,
                out var translation);
        return new XTransformComponent
        {
            LocalPosition = translation,
            LocalRotation = rotation,
            LocalScale = scale,
            LocalWorldMatrix = worldMatrix
        };
    }

    public static XTransformComponent CreateFromPosition(Vector3 position)
    {
        return new XTransformComponent
        {
            LocalPosition = position,
            LocalRotation = Quaternion.Identity,
            LocalScale = Vector3.One
        };
    }

    public void UpdateLocalMatrix()
    {
        var translationMatrix = Matrix4x4.CreateTranslation(LocalPosition);
        var rotationMatrix = Matrix4x4.CreateFromQuaternion(LocalRotation);
        var scaleMatrix = Matrix4x4.CreateScale(LocalScale);

        LocalWorldMatrix = scaleMatrix * rotationMatrix * translationMatrix;
    }
}
