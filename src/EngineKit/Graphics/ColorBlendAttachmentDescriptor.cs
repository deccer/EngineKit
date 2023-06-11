namespace EngineKit.Graphics;

public record struct ColorBlendAttachmentDescriptor(
    bool IsBlendEnabled,
    Blend SourceColorBlend,
    Blend DestinationColorBlend,
    BlendFunction ColorBlendFunction,
    Blend SourceAlphaBlend,
    Blend DestinationAlphaBlend,
    BlendFunction AlphaBlendFunction,
    ColorMask ColorWriteMask)
{
    public static readonly ColorBlendAttachmentDescriptor Opaque = new ColorBlendAttachmentDescriptor
    {
        IsBlendEnabled = false,
        SourceColorBlend = Blend.One,
        SourceAlphaBlend = Blend.One,
        DestinationColorBlend = Blend.Zero,
        DestinationAlphaBlend = Blend.Zero,
        ColorBlendFunction = BlendFunction.Add,
        AlphaBlendFunction = BlendFunction.Add,
        ColorWriteMask = ColorMask.All
    };

    public static readonly ColorBlendAttachmentDescriptor PreMultiplied = new ColorBlendAttachmentDescriptor
    {
        IsBlendEnabled = true,
        SourceColorBlend = Blend.SourceAlpha,
        SourceAlphaBlend = Blend.One,
        DestinationColorBlend = Blend.OneMinusSourceAlpha,
        DestinationAlphaBlend = Blend.OneMinusSourceAlpha,
        ColorBlendFunction = BlendFunction.Add,
        AlphaBlendFunction = BlendFunction.Add,
        ColorWriteMask = ColorMask.All
    };
    
    public static readonly ColorBlendAttachmentDescriptor Additive = new ColorBlendAttachmentDescriptor
    {
        IsBlendEnabled = true,
        SourceColorBlend = Blend.One,
        SourceAlphaBlend = Blend.One,
        DestinationColorBlend = Blend.SourceColor,
        DestinationAlphaBlend = Blend.SourceAlpha,
        ColorBlendFunction = BlendFunction.Add,
        AlphaBlendFunction = BlendFunction.Add,
        ColorWriteMask = ColorMask.All
    };
}