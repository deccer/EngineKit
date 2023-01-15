using OpenTK.Mathematics;

namespace SpaceGame.Game.Ecs.Components;

public class TransformComponent : Component
{
    public TransformComponent(Matrix4 modelMatrix)
    {
        LocalPosition = modelMatrix.ExtractTranslation();
        LocalRotation = modelMatrix.ExtractRotation();
        LocalScale = modelMatrix.ExtractScale();
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

    public Matrix4 GlobalWorldMatrix;

    public Matrix4 GetLocalMatrix()
    {
        return Matrix4.CreateScale(LocalScale) *
               Matrix4.CreateFromQuaternion(LocalRotation) *
               Matrix4.CreateTranslation(LocalPosition);
    }
}