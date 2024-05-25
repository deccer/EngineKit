using System;
using System.Numerics;
using EngineKit.Input;
using EngineKit.Mathematics;
using EngineKit.Native.Glfw;

namespace EngineKit;

public sealed class Camera : ICamera
{
    private readonly IApplicationContext _applicationContext;
    private readonly IInputProvider _inputProvider;
    private readonly Vector3 _worldUp;

    private Vector3 _position = Vector3.Zero;
    private Vector3 _front;
    private Vector3 _up;
    private Vector3 _right;

    private float _yaw;
    private float _pitch;

    private float _aspectRatio;

    private CameraMode _cameraMode;

    public float Zoom { get; set; } = 45.0f;

    public float Sensitivity { get; set; } = 0.5f;

    public float KeyboardAccelerationBoost { get; set; } = 30.0f;

    public float OptionalBoost { get; set; } = 6.0f;

    public Matrix4x4 ViewMatrix { get; private set; }

    public Matrix4x4 ProjectionMatrix { get; private set; }

    public float FieldOfView { get; set; }

    public float NearPlane { get; set; }

    public float FarPlane { get; set; }

    public Vector3 Velocity { get; set; }

    public Vector3 Position
    {
        get => _position;
        set
        {
            _position = value;
            UpdateCameraVectors();
        }
    }

    public Vector3 Direction => _front;

    public Vector3 Right => _right;

    public Vector3 Up
    {
        get => _up;
        set
        {
            _up = value;
            UpdateCameraVectors();
        }
    }

    public CameraMode Mode
    {
        get => _cameraMode;
        set
        {
            if (_cameraMode != value)
            {
                _cameraMode = value;
                UpdateCameraVectors();
            }
        }
    }

    public Camera(
        IApplicationContext applicationContext,
        IInputProvider inputProvider,
        Vector3 position,
        Vector3 up,
        CameraMode cameraMode = CameraMode.Perspective,
        float yaw = -90f,
        float pitch = 0.0f)
    {
        _applicationContext = applicationContext;
        _inputProvider = inputProvider;
        _worldUp = up;
        _yaw = yaw;
        _pitch = pitch;
        _front = new Vector3(0, 0, -1);
        _cameraMode = cameraMode;
        _position = position;
        _aspectRatio = 16.0f / 9.0f;

        FieldOfView = 60.0f;
        NearPlane = 0.1f;
        FarPlane = 1024f;

        UpdateCameraVectors();
    }

    public BoundingFrustum GetViewFrustum()
    {
        return new BoundingFrustum(ViewMatrix * ProjectionMatrix);
    }

    private Vector3 _acceleration;

    public void ProcessKeyboard()
    {
        if (_inputProvider.KeyboardState.IsKeyPressed(Glfw.Key.KeyW))
        {
            _acceleration += Direction * KeyboardAccelerationBoost;
        }

        if (_inputProvider.KeyboardState.IsKeyPressed(Glfw.Key.KeyS))
        {
            _acceleration -= Direction * KeyboardAccelerationBoost;
        }

        if (_inputProvider.KeyboardState.IsKeyPressed(Glfw.Key.KeyA))
        {
            _acceleration -= Right * KeyboardAccelerationBoost;
        }

        if (_inputProvider.KeyboardState.IsKeyPressed(Glfw.Key.KeyD))
        {
            _acceleration += Right * KeyboardAccelerationBoost;
        }

        if (_inputProvider.KeyboardState.IsKeyPressed(Glfw.Key.KeyLeftShift) ||
            _inputProvider.KeyboardState.IsKeyPressed(Glfw.Key.KeyRightShift))
        {
            _acceleration *= OptionalBoost;
        }

        if (_inputProvider.KeyboardState.IsKeyPressed(Glfw.Key.KeyLeftCtrl) ||
            _inputProvider.KeyboardState.IsKeyPressed(Glfw.Key.KeyRightCtrl))
        {
            _acceleration *= 1.0f / OptionalBoost;
        }
    }

    public void ProcessMouseMovement()
    {
        _yaw -= -_inputProvider.MouseState.DeltaX * Sensitivity;
        _pitch += _inputProvider.MouseState.DeltaY * Sensitivity;
        _pitch = _pitch.Clamp(-89.0f, 89.0f);

        UpdateCameraVectors();
    }

    public void AdvanceSimulation(float deltaTime)
    {
        Position += deltaTime * Velocity + 0.5f * _acceleration * deltaTime * deltaTime;
        Velocity += _acceleration * deltaTime;

        if (Velocity.Length() < 9.0f * deltaTime)
        {
            Velocity = new Vector3(0.0f);
        }

        const float dragConstant = 0.95f;
        var drag = MathF.Log10(dragConstant) * 144.0f;
        Velocity *= MathF.Exp(drag * deltaTime);

        _acceleration = Vector3.Zero;
    }

    public void Resize(int width, int height)
    {
        _aspectRatio = width / (float)height;
        UpdateCameraVectors();
    }

    private void UpdateCameraVectors()
    {
        if (_cameraMode != CameraMode.Orthogonal)
        {
            UpdateCameraVectorsForPerspective();
        }
        else
        {
            UpdateCameraVectorsForOrthogonal();
        }
    }

    private static Matrix4x4 CreateInfiniteReverseZPerspectiveRh(float fieldOfView, float aspectRatio, float nearPlane)
    {
        var t = 1.0f / MathF.Tan(fieldOfView / 2.0f);
        return new Matrix4x4(t / aspectRatio, 0.0f, 0.0f, 0.0f,
            0.0f, t, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, -1.0f, 0.0f,
            0.0f, nearPlane, 0.0f);
    }

    private void UpdateCameraVectorsForPerspective()
    {
        var eulerAngles = new Vector3
        {
            X = MathF.Cos(MathHelper.ToRadians(_yaw)) * MathF.Cos(MathHelper.ToRadians(_pitch)),
            Y = MathF.Sin(MathHelper.ToRadians(_pitch)),
            Z = MathF.Sin(MathHelper.ToRadians(_yaw)) * MathF.Cos(MathHelper.ToRadians(_pitch))
        };

        _front = Vector3.Normalize(eulerAngles);
        _right = Vector3.Normalize(Vector3.Cross(_front, _worldUp));
        _up = Vector3.Normalize(Vector3.Cross(_right, _front));

        ViewMatrix = Matrix4x4.CreateLookAt(_position, _position + _front, _up);
        if (_cameraMode == CameraMode.Perspective)
        {
            ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(FieldOfView),
                _aspectRatio,
                NearPlane,
                FarPlane);
        }
        else if (_cameraMode == CameraMode.PerspectiveInfinity)
        {
            ProjectionMatrix = CreateInfiniteReverseZPerspectiveRh(MathHelper.ToRadians(FieldOfView),
                _aspectRatio,
                NearPlane);
        }
    }

    private void UpdateCameraVectorsForOrthogonal()
    {
        var f = new Vector3
        {
            X = MathF.Cos(MathHelper.ToRadians(_yaw)) * MathF.Cos(MathHelper.ToRadians(_pitch)),
            Y = MathF.Sin(MathHelper.ToRadians(_pitch)),
            Z = MathF.Sin(MathHelper.ToRadians(_yaw)) * MathF.Cos(MathHelper.ToRadians(_pitch))
        };

        _front = Vector3.Normalize(f);
        _right = Vector3.Normalize(Vector3.Cross(_front, _worldUp));
        _up = Vector3.Normalize(Vector3.Cross(_right, _front));

        ViewMatrix = Matrix4x4.CreateLookAt(_position, _position + _front, _up);
        ProjectionMatrix = Matrix4x4.CreateOrthographic(
            _applicationContext.WindowScaledFramebufferSize.X,
            _applicationContext.WindowScaledFramebufferSize.Y,
            NearPlane,
            FarPlane);
    }
}
