using EngineKit.Mathematics;

namespace EngineKit.Graphics;

public record struct TextureCreateDescriptor
{
    public ImageType ImageType;

    public Format Format;

    public Int3 Size;

    public uint MipLevels;

    public uint ArrayLayers;

    public SampleCount SampleCount;

    public string? Label;
}