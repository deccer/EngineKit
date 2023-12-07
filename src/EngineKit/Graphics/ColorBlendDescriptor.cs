namespace EngineKit.Graphics;

internal record struct ColorBlendDescriptor(
    ColorBlendAttachmentDescriptor[] ColorBlendAttachmentDescriptors,
    float[] BlendConstants);