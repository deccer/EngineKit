namespace EngineKit.Graphics;

public record struct ColorBlendAttachmentDescriptor(
    bool IsBlendEnabled,
    BlendFactor SourceColorBlendFactor,
    BlendFactor DestinationColorBlendFactor,
    BlendOperation ColorBlendOperation,
    BlendFactor SourceAlphaBlendFactor,
    BlendFactor DestinationAlphaBlendFactor,
    BlendOperation AlphaBlendOperation,
    ColorMask ColorWriteMask)
{
    public static readonly ColorBlendAttachmentDescriptor Opaque = new ColorBlendAttachmentDescriptor
    {
        IsBlendEnabled = false,
        SourceColorBlendFactor = BlendFactor.One,
        DestinationColorBlendFactor = BlendFactor.Zero,
        ColorBlendOperation = BlendOperation.Add,
        SourceAlphaBlendFactor = BlendFactor.One,
        DestinationAlphaBlendFactor = BlendFactor.Zero,
        AlphaBlendOperation = BlendOperation.Add,
        ColorWriteMask = ColorMask.All
    };


    public static readonly ColorBlendAttachmentDescriptor PreMultiplied = new ColorBlendAttachmentDescriptor
    {
        IsBlendEnabled = true,
        SourceColorBlendFactor = BlendFactor.One,
        DestinationColorBlendFactor = BlendFactor.Zero,
        ColorBlendOperation = BlendOperation.Add,
        SourceAlphaBlendFactor = BlendFactor.One,
        DestinationAlphaBlendFactor = BlendFactor.Zero,
        AlphaBlendOperation = BlendOperation.Add,
        ColorWriteMask = ColorMask.All
    };
}