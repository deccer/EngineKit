using System;
using System.Numerics;
using EngineKit.Mathematics;

namespace Complex.Ecs;

public class Transform
{
    public bool IsDirty = true;
    private Vector3 _localPosition;
    private Vector3 _localRotation;
    private Vector3 _localScale;

    public Transform()
    {
        _localPosition = Vector3.Zero;
        _localRotation = Vector3.Zero;
        _localScale = Vector3.One;
    }

    public Vector3 LocalPosition
    {
        get => _localPosition;
        set
        {
            if (_localPosition != value)
            {
                _localPosition = value;
                IsDirty = true;
            }
        }
    }

    public Vector3 LocalRotation
    {
        get => _localRotation;
        set
        {
            if (_localRotation != value)
            {
                _localRotation = value;
                IsDirty = true;
            }
        }
    }

    public Vector3 LocalScale
    {
        get => _localScale;
        set
        {
            if (_localScale != value)
            {
                _localScale = value;
                IsDirty = true;
            }
        }
    }

    public Matrix4x4 GlobalWorldMatrix;

    public void SetLocalTransform(Matrix4x4 localWorldMatrix)
    {
        Matrix4x4.Decompose(localWorldMatrix, out var localScale, out var localRotation, out var localTranslation);

        LocalPosition = localTranslation;
        LocalRotation = QuaternionToEulerAngles(localRotation);
        LocalScale = localScale;
    }
    
    public void SetGlobalTransform(Matrix4x4 localWorldMatrix)
    {
        GlobalWorldMatrix = GlobalWorldMatrix;
        IsDirty = false;
    }
    
    public void ComputeGlobalModelMatrix()
    {
        GlobalWorldMatrix = GetLocalWorldMatrix();
        IsDirty = false;
    }

    public void ComputeGlobalModelMatrix(ref Matrix4x4 parentGlobalWorldMatrix)
    {
        GlobalWorldMatrix = parentGlobalWorldMatrix * GetLocalWorldMatrix();
        IsDirty = false;
    }

    private Matrix4x4 GetLocalWorldMatrix()
    {
        var rotationX = Matrix4x4.CreateRotationX(MathHelper.ToRadians(LocalRotation.X));
        var rotationY = Matrix4x4.CreateRotationY(MathHelper.ToRadians(LocalRotation.Y));
        var rotationZ = Matrix4x4.CreateRotationZ(MathHelper.ToRadians(LocalRotation.Z));
        var rotationMatrix = rotationY * rotationX * rotationZ;
        
        return Matrix4x4.CreateTranslation(LocalPosition) * rotationMatrix * Matrix4x4.CreateScale(LocalScale);
    }

    private Vector3 QuaternionToEulerAngles(Quaternion q)
    {
        Vector3 angles;
        var sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
        var cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
        angles.Z = MathF.Atan2(sinr_cosp, cosr_cosp);

        // pitch (y-axis rotation)
        var sinp = MathF.Sqrt(1 + 2 * (q.W * q.Y - q.X * q.Z));
        var cosp = MathF.Sqrt(1 - 2 * (q.W * q.Y - q.X * q.Z));
        angles.X = 2 * MathF.Atan2(sinp, cosp) - MathHelper.Pi / 2.0f;

        // yaw (z-axis rotation)
        var siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
        var cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
        angles.Y = MathF.Atan2(siny_cosp, cosy_cosp);
        return angles;
    }
   
}