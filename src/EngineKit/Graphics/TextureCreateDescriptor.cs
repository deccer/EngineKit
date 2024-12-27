using EngineKit.Graphics.RHI;
using EngineKit.Mathematics;

namespace EngineKit.Graphics;

public record struct TextureCreateDescriptor(
    TextureType TextureType,
    Format Format,
    Int3 Size,
    uint MipLevels,
    uint ArrayLayers,
    TextureSampleCount TextureSampleCount,
    string? Label);
