using System.Collections.Generic;
using OpenTK.Mathematics;

namespace EngineKit.Graphics;

public sealed class FramebufferRenderDescriptorBuilder
{
    private readonly IList<RenderAttachment> _renderAttachments;
    private FramebufferRenderDescriptor _framebufferRenderDescriptor;

    public FramebufferRenderDescriptorBuilder()
    {
        _framebufferRenderDescriptor = new FramebufferRenderDescriptor();
        _renderAttachments = new List<RenderAttachment>();
    }

    public FramebufferRenderDescriptorBuilder WithViewport(int width, int height)
    {
        _framebufferRenderDescriptor.Viewport = new OpenTK.Mathematics.Vector4i(0, 0, width, height);
        return this;
    }

    public FramebufferRenderDescriptorBuilder WithColorAttachment(ITexture colorAttachment, bool clear,
        Vector4 clearColor)
    {
        var clearValue = new ClearValue(clearColor.X, clearColor.Y, clearColor.Z, clearColor.W, 1.0f, 0);
        _renderAttachments.Add(new RenderAttachment(colorAttachment, clearValue, clear));

        return this;
    }

    public FramebufferRenderDescriptorBuilder WithDepthAttachment(ITexture depthTexture, bool clear, float clearValue = 1.0f)
    {
        _framebufferRenderDescriptor.DepthAttachment = new RenderAttachment(depthTexture, new ClearValue(0, 0, 0, 0, clearValue, 0), clear);
        return this;
    }

    public FramebufferRenderDescriptorBuilder WithStencilAttachment(ITexture stencilTexture, bool clear, int clearValue = 0)
    {
        _framebufferRenderDescriptor.StencilAttachment = new RenderAttachment(stencilTexture, new ClearValue(0, 0, 0, 0, 0.0f, clearValue), clear);
        return this;
    }

    public FramebufferRenderDescriptor Build(string label)
    {
        _framebufferRenderDescriptor.ColorAttachments = new RenderAttachment[_renderAttachments.Count];
        for (var i = 0; i < _renderAttachments.Count; i++)
        {
            _framebufferRenderDescriptor.ColorAttachments[i] = _renderAttachments[i];
        }
        _framebufferRenderDescriptor.Label = label;
        return _framebufferRenderDescriptor;
    }
}