using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

public readonly record struct VertexInputBindingDescriptor(
    uint Location,
    uint Binding,
    GL.DataType DataType,
    int ComponentCount,
    uint Offset,
    bool IsNormalized = false);