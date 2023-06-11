using EngineKit.Mathematics;

namespace DeferredRendering;

public interface ICamera
{
    Matrix ViewMatrix { get; }

    Matrix ProjectionMatrix { get; }

    Vector3 Position { get; }

    Vector3 Direction { get; }

    Vector3 Right { get; }

    Vector3 Up { get; set; }

    float Speed { get; set; }

    float Sensitivity { get; set; }

    float Zoom { get; set; }

    void ProcessKeyboard(Vector3 movement, float deltaTime);

    void ProcessMouseMovement();

    void Resize();
}