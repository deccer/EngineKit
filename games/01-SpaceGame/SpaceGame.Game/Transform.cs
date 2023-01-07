using OpenTK.Mathematics;
using Quaternion = OpenTK.Mathematics.Quaternion;
using Vector3 = OpenTK.Mathematics.Vector3;
using Vector4 = OpenTK.Mathematics.Vector4;

namespace SpaceGame.Game;

public class Transform
{
    private readonly Transform? _parent;

    public Vector3 LocalPosition = Vector3.Zero;

    private Vector3 LocalScale = Vector3.One;

    public Quaternion LocalRotation = Quaternion.Identity;

    public Matrix4 LocalToWorld;

    public Matrix4 WorldToLocal;

    public Vector3 WorldPosition
    {
        get { return Vector3.TransformPosition(Vector3.Zero, LocalToWorld); }
        set { LocalPosition = Vector3.TransformPosition(value, WorldToLocal); }
    }

    public Quaternion WorldRotation
    {
        get { return Quaternion.FromMatrix(new Matrix3(LocalToWorld)); }
    }

    public Vector3 Forward
    {
        get { return Vector3.TransformNormal(-Vector3.UnitZ, LocalToWorld); }
    }

    public Vector3 Right
    {
        get { return Vector3.TransformNormal(Vector3.UnitX, LocalToWorld); }
    }

    public Vector3 Up
    {
        get { return Vector3.TransformNormal(Vector3.UnitY, LocalToWorld); }
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

    public Matrix4 GetTransformationMatrix()
    {
        Matrix4 matrix = default;
        Matrix3.CreateFromQuaternion(in LocalRotation, out var rotationMatrix);

        matrix.Row0 = new Vector4(rotationMatrix.Column0 * LocalScale.X, 0f);
        matrix.Row1 = new Vector4(rotationMatrix.Column1 * LocalScale.Y, 0f);
        matrix.Row2 = new Vector4(rotationMatrix.Column2 * LocalScale.Z, 0f);
        matrix.Row3 = new Vector4(LocalPosition, 1f);
        if (_parent != null)
        {
            var parentMatrix = _parent.GetTransformationMatrix();
            Matrix4.Mult(matrix, parentMatrix, out matrix);
        }

        return matrix;
    }

    public Vector3 LocalDirectionToWorld(Vector3 localDir)
    {
        return Vector3.TransformNormal(localDir, LocalToWorld);
    }

    public Vector3 WorldDirectionToLocal(Vector3 worldDir)
    {
        return Vector3.TransformNormal(worldDir, WorldToLocal);
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