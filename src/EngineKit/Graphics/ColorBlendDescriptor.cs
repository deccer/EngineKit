namespace EngineKit.Graphics;

public record struct ColorBlendDescriptor(
    ColorBlendAttachmentDescriptor[] ColorBlendAttachmentDescriptors,
    float[] BlendConstants);