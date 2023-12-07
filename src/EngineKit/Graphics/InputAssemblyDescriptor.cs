namespace EngineKit.Graphics;

internal record struct InputAssemblyDescriptor(
    PrimitiveTopology PrimitiveTopology,
    bool IsPrimitiveRestartEnabled);