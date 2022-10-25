namespace EngineKit.Graphics;

public record struct VertexInputBindingDescriptor(
    uint Location,
    uint Binding,
    int ComponentCount,
    DataType DataType,
    uint Offset,
    bool IsNormalized = false);