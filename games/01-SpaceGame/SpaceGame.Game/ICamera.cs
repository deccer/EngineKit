using EngineKit.Mathematics;

namespace SpaceGame.Game;

public interface ICamera
{
    Matrix ViewMatrix { get; }

    Matrix ProjectionMatrix { get; }

    Vector3 Position { get; set; }

    Vector3 Direction { get; }

    Vector3 Right { get; }

    Vector3 Up { get; set; }

    float FieldOfView { get; set; }

    float AspectRatio { get; }

    float NearPlane { get; set; }

    float FarPlane { get; set; }

    void ProcessKeyboard(Vector3 movement, float deltaTime);

    void ProcessMouseMovement();

    void UpdateCameraVectors();
}