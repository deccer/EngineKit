using OpenTK.Mathematics;

namespace SpaceGame.Game.Ecs;

public class TransformComponent : Component
{
    public static TransformComponent CreateFromMatrix(Matrix4 worldMatrix)
    {
        var scale = worldMatrix.ExtractScale();
        /*
        scale.X = 1.0f / scale.X;
        scale.Y = 1.0f / scale.Y;
        scale.Z = 1.0f / scale.Z;
        */
        return new TransformComponent
        {
            LocalPosition = worldMatrix.ExtractTranslation(),
            LocalRotation = worldMatrix.ExtractRotation(),
            LocalScale = Vector3.One
        };
    }

    public Vector3 LocalPosition;

    public Quaternion LocalRotation;

    public Vector3 LocalScale;

    public Vector3 GlobalPosition;

    public Quaternion GlobalRotation;

    public Vector3 GlobalScale;

    public Matrix4 GlobalWorldMatrix;
}