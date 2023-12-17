﻿using System.Numerics;

namespace CullOnGpu;

public interface ICamera
{
    Matrix4x4 ViewMatrix { get; }

    Matrix4x4 ProjectionMatrix { get; }

    Vector3 Position { get; }

    Vector3 Direction { get; }

    Vector3 Right { get; }

    Vector3 Up { get; set; }

    float Speed { get; set; }

    float Sensitivity { get; set; }

    float Zoom { get; set; }
    
    CameraMode Mode { get; set; }

    void ProcessKeyboard(Vector3 movement, float deltaTime);

    void ProcessMouseMovement();

    void Resize();
}