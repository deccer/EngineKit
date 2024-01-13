using System.Numerics;
using EngineKit.Mathematics;

namespace EngineKit;

public interface ICamera
{
    Matrix4x4 ViewMatrix { get; }

    Matrix4x4 ProjectionMatrix { get; }

    Vector3 Position { get; }

    Vector3 Direction { get; }

    Vector3 Right { get; }

    Vector3 Up { get; set; }

    float KeyboardAccelerationBoost { get; set; }
    
    float OptionalBoost { get; set; }

    float Sensitivity { get; set; }

    float Zoom { get; set; }
    
    CameraMode Mode { get; set; }
    
    Vector3 Velocity { get; set; }

    void ProcessKeyboard();

    void ProcessMouseMovement();

    void Resize();

    BoundingFrustum GetViewFrustum();
    void AdvanceSimulation(float deltaTime);
}