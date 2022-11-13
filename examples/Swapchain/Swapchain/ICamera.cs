﻿using OpenTK.Mathematics;

namespace Swapchain;

public interface ICamera
{
    Matrix4 ViewMatrix { get; }

    Matrix4 ProjectionMatrix { get; }

    Vector3 Position { get; }

    Vector3 Direction { get; }

    Vector3 Right { get; }

    Vector3 Up { get; set; }

    void ProcessKeyboard(Vector3 movement, float deltaTime);

    void ProcessMouseMovement();

    void Resize();
}