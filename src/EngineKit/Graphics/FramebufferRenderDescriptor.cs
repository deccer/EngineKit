using EngineKit.Mathematics;

namespace EngineKit.Graphics;

public record struct FramebufferRenderDescriptor
{
    public string Label;

    public Viewport Viewport;

    public RenderAttachment[] ColorAttachments;

    public RenderAttachment? DepthAttachment;

    public RenderAttachment? StencilAttachment;
}