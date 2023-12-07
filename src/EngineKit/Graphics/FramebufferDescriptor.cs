using System.Linq;
using EngineKit.Mathematics;

namespace EngineKit.Graphics;

public record struct FramebufferDescriptor
{
    public string Label;

    public Viewport Viewport;

    public FramebufferRenderAttachment[] ColorAttachments;

    public FramebufferRenderAttachment? DepthAttachment;

    public FramebufferRenderAttachment? StencilAttachment;
    
    public bool HasSrgbEnabledAttachment()
    {
        return ColorAttachments.Any(colorAttachment => colorAttachment.Texture.TextureCreateDescriptor.Format is Format.R8G8B8Srgb or Format.R8G8B8A8Srgb);
    }
}