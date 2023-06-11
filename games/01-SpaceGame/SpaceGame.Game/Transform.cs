using EngineKit.Mathematics;
using Quaternion = EngineKit.Mathematics.Quaternion;
using Vector3 = EngineKit.Mathematics.Vector3;
using Vector4 = EngineKit.Mathematics.Vector4;

namespace SpaceGame.Game;

public class Transform
{
    private readonly Transform? _parent;

    public Vector3 LocalPosition = Vector3.Zero;

    private Vector3 LocalScale = Vector3.One;

    public Quaternion LocalRotation = Quaternion.Identity;

    public Matrix LocalToWorld;

    public Matrix WorldToLocal;

    public Vector3 WorldPosition
    {
        get { return Vector3.TransformPosition(Vector3.Zero, LocalToWorld); }
        set { LocalPosition = Vector3.TransformPosition(value, WorldToLocal); }
    }

    public Quaternion WorldRotation
    {
        get { return Quaternion.RotationMatrix(LocalToWorld); }
    }

    public Vector3 Forward
    {
        get { return Vector3.TransformDirection(-Vector3.UnitZ, LocalToWorld); }
    }

    public Vector3 Right
    {
        get { return Vector3.TransformDirection(Vector3.UnitX, LocalToWorld); }
    }

    public Vector3 Up
    {
        get { return Vector3.TransformDirection(Vector3.UnitY, LocalToWorld); }
    }

    public Transform(
        Transform? parent,
        Vector3? position,
        Quaternion? rotation,
        Vector3? scale)
    {
        _parent = parent;
        LocalPosition = position ?? Vector3.Zero;
        LocalRotation = rotation ?? Quaternion.Identity;
        LocalScale = Vector3.One;
    }

    public void UpdateMatrices()
    {
        LocalToWorld = GetTransformationMatrix();
        WorldToLocal = LocalToWorld;
        WorldToLocal.Invert();
    }

    public Matrix GetTransformationMatrix()
    {
        Matrix matrix = default;
        Matrix3x3.RotationQuaternion(ref LocalRotation, out var rotationMatrix);

        matrix.Row1 = new Vector4(rotationMatrix.Column1 * LocalScale.X, 0f);
        matrix.Row2 = new Vector4(rotationMatrix.Column2 * LocalScale.Y, 0f);
        matrix.Row3 = new Vector4(rotationMatrix.Column3 * LocalScale.Z, 0f);
        matrix.Row4 = new Vector4(LocalPosition, 1f);
        if (_parent != null)
        {
            var parentMatrix = _parent.GetTransformationMatrix();
            matrix = Matrix.Multiply(matrix, parentMatrix);
        }

        return matrix;
    }

    public Vector3 LocalDirectionToWorld(Vector3 localDir)
    {
        return Vector3.TransformDirection(localDir, LocalToWorld);
    }

    public Vector3 WorldDirectionToLocal(Vector3 worldDir)
    {
        return Vector3.TransformDirection(worldDir, WorldToLocal);
    }

    public Vector3 LocalPositionToWorld(in Vector3 localPos)
    {
        return Vector3.TransformPosition(localPos, LocalToWorld);
    }

    public Vector3 WorldPositionToLocal(in Vector3 worldPos)
    {
        return Vector3.TransformPosition(worldPos, WorldToLocal);
    }
}