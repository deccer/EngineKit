namespace EngineKit.Graphics;

public readonly record struct VertexInputBindingDescriptor(
    uint Location,
    uint Binding,
    DataType DataType,
    int ComponentCount,
    uint Offset,
    bool IsNormalized = false);