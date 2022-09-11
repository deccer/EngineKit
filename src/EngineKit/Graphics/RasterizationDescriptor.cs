namespace EngineKit.Graphics;

public record struct RasterizationDescriptor(
    bool IsDepthClampEnabled = false,
    bool IsDepthBiasEnabled = false,
    float DepthBiasConstantFactor = 0.0f,
    float DepthBiasSlopeFactor = 0.0f,
    float LineWidth = 1.0f,
    float PointSize = 1.0f,
    FillMode FillMode = FillMode.Solid,
    bool IsCullingEnabled = true,
    CullMode CullMode = CullMode.Back,
    FaceWinding FaceWinding = FaceWinding.CounterClockwise);