using OpenTK.Mathematics;

namespace EngineKit.Graphics;

public record struct FramebufferRenderDescriptor
{
    public string Label;

    public Vector4i Viewport;

    public RenderAttachment[] ColorAttachments;

    public RenderAttachment? DepthAttachment;

    public RenderAttachment? StencilAttachment;
}