using System.Numerics;
using OpenSpace;

namespace ComplexExample;

public interface ICamera
{
    CameraMode CameraMode { get; set; }

    Matrix4x4 ViewMatrix { get; }

    Matrix4x4 ProjectionMatrix { get; }

    Vector3 Position { get; set; }

    Vector3 Direction { get; }

    Vector3 Right { get; }

    Vector3 Up { get; set; }

    float Speed { get; set; }

    float Sensitivity { get; set; }

    float Zoom { get; set; }
    
    public float AspectRatio { get; }
    
    public float FieldOfView { get; set; }

    public float NearPlane { get; set; }

    public float FarPlane { get; set; }

    void ProcessKeyboard(Vector3 movement, float deltaTime);

    void ProcessMouseMovement();

    void Resize();

    void RestoreState();

    void BackupState();
}