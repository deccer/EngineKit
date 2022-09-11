namespace EngineKit.Graphics;

public record struct VertexBindingDescriptor(
    uint Location,
    uint Binding,
    int ComponentCount,
    DataType DataType,
    uint Offset,
    bool IsNormalized = false);