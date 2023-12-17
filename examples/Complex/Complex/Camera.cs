using System;
using System.Numerics;
using EngineKit;
using EngineKit.Input;
using MathHelper = EngineKit.MathHelper;

namespace Complex;

public sealed class Camera : ICamera
{
    public float Speed { get; set; } = 1.2f;

    public float Sensitivity { get; set; } = 0.5f;

    public float Zoom { get; set; } = 45.0f;

    private readonly IApplicationContext _applicationContext;
    private readonly IInputProvider _inputProvider;
    private readonly Vector3 _worldUp;
    private Vector3 _position = Vector3.Zero;
    private Vector3 _front;
    private Vector3 _up;
    private Vector3 _right;

    private float _yaw;
    private float _pitch;

    private CameraMode _cameraMode;

    public Matrix4x4 ViewMatrix { get; private set; }

    public Matrix4x4 ProjectionMatrix { get; private set; }

    public float FieldOfView { get; set; }

    public float NearPlane { get; set; }

    public float FarPlane { get; set; }

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
        FieldOfView = 60.0f;
        NearPlane = 0.1f;
        FarPlane = 1024f;
        UpdateCameraVectors();
    }

    public void ProcessKeyboard(Vector3 movement, float deltaTime)
    {
        var velocity = Speed * deltaTime;
        _position += movement * velocity;
        UpdateCameraVectors();
    }

    public void ProcessMouseMovement()
    {
        _yaw -= -_inputProvider.MouseState.DeltaX * Sensitivity;
        _pitch += _inputProvider.MouseState.DeltaY * Sensitivity;
        _pitch = _pitch.Clamp(-89.0f, 89.0f);

        UpdateCameraVectors();
    }

    public void Resize()
    {
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
        var f = 1.0f / MathF.Tan(fieldOfView / 2.0f);
        return new Matrix4x4(f / aspectRatio, 0.0f, 0.0f, 0.0f,
            0.0f, f, 0.0f, 0.0f, 0.0f,
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
                _applicationContext.ScaledFramebufferSize.X / (float)_applicationContext.ScaledFramebufferSize.Y,
                NearPlane,
                FarPlane);
        }
        else if (_cameraMode == CameraMode.PerspectiveInfinity)
        {
            ProjectionMatrix = CreateInfiniteReverseZPerspectiveRh(MathHelper.ToRadians(FieldOfView),
                _applicationContext.ScaledFramebufferSize.X / (float)_applicationContext.ScaledFramebufferSize.Y,
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
            _applicationContext.ScaledFramebufferSize.X,
            _applicationContext.ScaledFramebufferSize.Y,
            NearPlane,
            FarPlane);
    }
}