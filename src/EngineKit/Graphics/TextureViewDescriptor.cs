using EngineKit.Graphics.RHI;

namespace EngineKit.Graphics;

public record struct TextureViewDescriptor
{
    public TextureType TextureType;
    
    public Format Format;

    public uint MinLevel;

    public uint NumLevels;

    public uint MinLayer;

    public uint NumLayers;

    public string? Label;

    public SwizzleMapping SwizzleMapping;
}