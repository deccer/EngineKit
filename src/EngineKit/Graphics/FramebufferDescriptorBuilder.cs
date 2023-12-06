using System.Collections.Generic;
using System.Numerics;
using EngineKit.Mathematics;

namespace EngineKit.Graphics;

public sealed class FramebufferDescriptorBuilder
{
    private readonly IList<FramebufferRenderAttachment> _renderAttachments;
    private FramebufferDescriptor _framebufferDescriptor;

    public FramebufferDescriptorBuilder()
    {
        _framebufferDescriptor = new FramebufferDescriptor();
        _renderAttachments = new List<FramebufferRenderAttachment>();
    }

    public FramebufferDescriptorBuilder WithViewport(int width, int height, float minDepthRange = -1.0f, float maxDepthRange = 1.0f)
    {
        _framebufferDescriptor.Viewport = new Viewport(0, 0, width, height);
        _framebufferDescriptor.Viewport.MinDepth = minDepthRange;
        _framebufferDescriptor.Viewport.MaxDepth = maxDepthRange;
        return this;
    }

    public FramebufferDescriptorBuilder WithColorAttachment(
        ITexture colorAttachment,
        bool clear,
        int clearColor)
    {
        var clearValue = new ClearValue(clearColor, 0, 0, 0, 1.0f, 0);
        _renderAttachments.Add(new FramebufferRenderAttachment(colorAttachment, clearValue, clear));

        return this;
    }

    public FramebufferDescriptorBuilder WithColorAttachment(
        ITexture colorAttachment,
        bool clear,
        uint clearColor)
    {
        var clearValue = new ClearValue(clearColor, 0, 0, 0, 1.0f, 0);
        _renderAttachments.Add(new FramebufferRenderAttachment(colorAttachment, clearValue, clear));

        return this;
    }

    public FramebufferDescriptorBuilder WithColorAttachment(
        ITexture colorAttachment,
        bool clear,
        Int2 clearColor)
    {
        var clearValue = new ClearValue(clearColor.X, clearColor.Y, 0, 0, 1.0f, 0);
        _renderAttachments.Add(new FramebufferRenderAttachment(colorAttachment, clearValue, clear));

        return this;
    }

    public FramebufferDescriptorBuilder WithColorAttachment(
        ITexture colorAttachment,
        bool clear,
        Int3 clearColor)
    {
        var clearValue = new ClearValue(clearColor.X, clearColor.Y, clearColor.Z, 0, 1.0f, 0);
        _renderAttachments.Add(new FramebufferRenderAttachment(colorAttachment, clearValue, clear));

        return this;
    }

    public FramebufferDescriptorBuilder WithColorAttachment(
        ITexture colorAttachment,
        bool clear,
        Int4 clearColor)
    {
        var clearValue = new ClearValue(clearColor.X, clearColor.Y, clearColor.Z, clearColor.W, 1.0f, 0);
        _renderAttachments.Add(new FramebufferRenderAttachment(colorAttachment, clearValue, clear));

        return this;
    }

    public FramebufferDescriptorBuilder WithColorAttachment(
        ITexture colorAttachment,
        bool clear,
        float clearColor)
    {
        var clearValue = new ClearValue(clearColor, 0.0f, 0.0f, 0.0f, 1.0f, 0);
        _renderAttachments.Add(new FramebufferRenderAttachment(colorAttachment, clearValue, clear));

        return this;
    }

    public FramebufferDescriptorBuilder WithColorAttachment(
        ITexture colorAttachment,
        bool clear,
        Vector2 clearColor)
    {
        var clearValue = new ClearValue(clearColor.X, clearColor.Y, 0.0f, 0.0f, 1.0f, 0);
        _renderAttachments.Add(new FramebufferRenderAttachment(colorAttachment, clearValue, clear));

        return this;
    }

    public FramebufferDescriptorBuilder WithColorAttachment(
        ITexture colorAttachment,
        bool clear,
        Vector3 clearColor)
    {
        var clearValue = new ClearValue(clearColor.X, clearColor.Y, clearColor.Z, 0.0f, 1.0f, 0);
        _renderAttachments.Add(new FramebufferRenderAttachment(colorAttachment, clearValue, clear));

        return this;
    }

    public FramebufferDescriptorBuilder WithColorAttachment(
        ITexture colorAttachment,
        bool clear,
        Vector4 clearColor)
    {
        var clearValue = new ClearValue(clearColor.X, clearColor.Y, clearColor.Z, clearColor.W, 1.0f, 0);
        _renderAttachments.Add(new FramebufferRenderAttachment(colorAttachment, clearValue, clear));

        return this;
    }

    public FramebufferDescriptorBuilder WithDepthAttachment(
        ITexture depthTexture,
        bool clear,
        float clearValue = 1.0f)
    {
        _framebufferDescriptor.DepthAttachment = new FramebufferRenderAttachment(depthTexture, new ClearValue(0, 0, 0, 0, clearValue, 0), clear);
        return this;
    }

    public FramebufferDescriptorBuilder WithStencilAttachment(
        ITexture stencilTexture,
        bool clear,
        int clearValue = 0)
    {
        _framebufferDescriptor.StencilAttachment = new FramebufferRenderAttachment(stencilTexture, new ClearValue(0, 0, 0, 0, 0.0f, clearValue), clear);
        return this;
    }

    public FramebufferDescriptor Build(string label)
    {
        _framebufferDescriptor.ColorAttachments = new FramebufferRenderAttachment[_renderAttachments.Count];
        for (var i = 0; i < _renderAttachments.Count; i++)
        {
            _framebufferDescriptor.ColorAttachments[i] = _renderAttachments[i];
        }
        _framebufferDescriptor.Label = label;
        return _framebufferDescriptor;
    }
}