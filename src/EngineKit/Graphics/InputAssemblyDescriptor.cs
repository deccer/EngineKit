namespace EngineKit.Graphics;

public record struct InputAssemblyDescriptor(
    PrimitiveTopology PrimitiveTopology,
    bool IsPrimitiveRestartEnabled);