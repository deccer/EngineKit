﻿using System;
using EngineKit;
using EngineKit.Input;
using OpenTK.Mathematics;
using MathHelper = EngineKit.MathHelper;

namespace SpaceGame.Game;

public sealed class Camera : ICamera
{
    private const float Speed = 1.2f;
    private const float Sensitivity = 0.5f;
    private const float Zoom = 45.0f;

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

    public float AspectRatio => _aspectRatio;

    public float FieldOfView { get; set; }

    public float NearPlane { get; set; }

    public float FarPlane { get; set; }

    public Matrix4 ViewMatrix { get; private set; }

    public Matrix4 ProjectionMatrix { get; private set; }

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

    private Camera()
    {

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
        FarPlane = 2048f;
        _aspectRatio = 1.0f;
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
        if (_pitch > 89.9f)
        {
            _pitch = 89.9f;
        }

        if (_pitch < -89.9f)
        {
            _pitch = -89.9f;
        }

        UpdateCameraVectors();
    }

    public void UpdateCameraVectors()
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

        _aspectRatio = _applicationContext.ScaledFramebufferSize.X / (float)_applicationContext.ScaledFramebufferSize.Y;

        ViewMatrix = Matrix4.LookAt(_position, _position + _front, _up);
        ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(
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

        ViewMatrix = Matrix4.LookAt(_position, _position + _front, _up);
        ProjectionMatrix = Matrix4.CreateOrthographic(
            _applicationContext.ScaledFramebufferSize.X,
            _applicationContext.ScaledFramebufferSize.Y,
            NearPlane,
            FarPlane);
    }
}