using EngineKit.Mathematics;

namespace EngineKit.Graphics;

public sealed class FramebufferRenderDescriptorBuilder
{
    private FramebufferRenderDescriptor _framebufferRenderDescriptor;

    public FramebufferRenderDescriptorBuilder()
    {
        _framebufferRenderDescriptor = new FramebufferRenderDescriptor();
    }

    public FramebufferRenderDescriptorBuilder WithViewport(int width, int height)
    {
        _framebufferRenderDescriptor.Viewport = new Viewport(0, 0, width, height);
        return this;
    }

    public FramebufferRenderDescriptorBuilder WithColorAttachments(Vector4 clearColor, bool clear, params ITexture[] colorTextures)
    {
        var clearValue = new ClearValue(clearColor.X, clearColor.Y, clearColor.Z, clearColor.W, 1.0f, 0);
        _framebufferRenderDescriptor.ColorAttachments = new RenderAttachment[colorTextures.Length];
        for (var colorAttachmentIndex = 0; colorAttachmentIndex < colorTextures.Length; colorAttachmentIndex++)
        {
            _framebufferRenderDescriptor.ColorAttachments[colorAttachmentIndex] =
                new RenderAttachment(colorTextures[colorAttachmentIndex], clearValue, clear);
        }

        return this;
    }

    public FramebufferRenderDescriptorBuilder WithDepthAttachment(ITexture depthTexture, bool clear)
    {
        _framebufferRenderDescriptor.DepthAttachment = new RenderAttachment(depthTexture, ClearValue.White, clear);
        return this;
    }

    public FramebufferRenderDescriptorBuilder WithStencilAttachment(ITexture stencilTexture, bool clear)
    {
        _framebufferRenderDescriptor.StencilAttachment = new RenderAttachment(stencilTexture, ClearValue.Zero, clear);
        return this;
    }

    public FramebufferRenderDescriptor Build(string label)
    {
        _framebufferRenderDescriptor.Label = label;
        return _framebufferRenderDescriptor;
    }
}