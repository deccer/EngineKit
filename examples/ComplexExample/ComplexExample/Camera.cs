using System;
using System.Numerics;
using EngineKit;
using EngineKit.Input;
using OpenSpace;
using MathHelper = EngineKit.Mathematics.MathHelper;

namespace ComplexExample;

internal sealed class Camera : ICamera
{
    public float Speed { get; set; } = 1.2f;

    public float Sensitivity { get; set; } = 0.15f;

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

    private float _backupYaw;
    private float _backupPitch;
    private Vector3 _backupPosition;
    private Vector3 _backupFront;
    private Vector3 _backupUp;
    private float _backupNearPlane;
    private float _backupFarPlane;
    private float _aspectRatio;

    public Matrix4x4 ViewMatrix { get; private set; }

    public Matrix4x4 ProjectionMatrix { get; private set; }
    
    public float AspectRatio => _aspectRatio;

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

    public CameraMode CameraMode
    {
        get => _cameraMode;
        set => _cameraMode = value;
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

    public void BackupState()
    {
        _backupYaw = _yaw;
        _backupPitch = _pitch;
        _backupPosition = _position;
        _backupFront = _front;
        _backupUp = _up;
        _backupNearPlane = NearPlane;
        _backupFarPlane = FarPlane;
    }

    public void RestoreState()
    {
        _yaw = _backupYaw;
        _pitch = _backupPitch;
        _position = _backupPosition;
        _front = _backupFront;
        _up = _backupUp;
        NearPlane = _backupNearPlane;
        FarPlane = _backupFarPlane;
    }

    private void UpdateCameraVectors()
    {
        if (_cameraMode == CameraMode.Perspective)
        {
            UpdateCameraVectorsForPerspective();
        }
        else
        {
            UpdateCameraVectorsForOrthogonal();
        }
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
        _aspectRatio = _applicationContext.ScaledFramebufferSize.X / (float)_applicationContext.ScaledFramebufferSize.Y;
        ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(FieldOfView),
            _aspectRatio,
            NearPlane,
            FarPlane);
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
        _aspectRatio = 1.0f;

        var width = _applicationContext.ScaledFramebufferSize.X;
        var height = _applicationContext.ScaledFramebufferSize.Y;
        ProjectionMatrix = Matrix4x4.CreateOrthographicOffCenter(
            -width / 2.0f * Zoom,
            width / 2.0f * Zoom,
            -height / 2.0f * Zoom,
            height / 2.0f * Zoom,
            NearPlane,
            FarPlane);
    }
}