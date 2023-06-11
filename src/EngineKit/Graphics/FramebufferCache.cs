using System.Collections.Generic;
using System.Linq;
using EngineKit.Native.OpenGL;
using Serilog;

namespace EngineKit.Graphics;

internal sealed class FramebufferCache : IFramebufferCache
{
    private readonly ILogger _logger;
    private readonly IDictionary<FramebufferDescriptor, uint> _framebufferCache;

    public FramebufferCache(ILogger logger)
    {
        _logger = logger.ForContext<FramebufferCache>();
        _framebufferCache = new Dictionary<FramebufferDescriptor, uint>(16);
    }

    public void Dispose()
    {
        foreach (var fbo in _framebufferCache.Values)
        {
            GL.DeleteFramebuffer(fbo);
        }

        _framebufferCache.Clear();
    }

    public uint GetOrCreateFramebuffer(FramebufferDescriptor framebufferDescriptor)
    {
        return _framebufferCache.TryGetValue(framebufferDescriptor, out var framebuffer)
            ? framebuffer
            : CreateFramebuffer(framebufferDescriptor);
    }

    public void RemoveFramebuffer(FramebufferDescriptor framebufferDescriptor)
    {
        if (_framebufferCache.TryGetValue(framebufferDescriptor, out var framebuffer))
        {
            GL.DeleteFramebuffer(framebuffer);
            _framebufferCache.Remove(framebufferDescriptor);
        }
    }

    private uint CreateFramebuffer(FramebufferDescriptor framebufferDescriptor)
    {
        var framebufferName = "Framebuffer-" + framebufferDescriptor.Label;
        var drawBuffers = new List<GL.DrawBuffer>(8);
        var framebufferId = GL.CreateFramebuffer();
        if (!string.IsNullOrEmpty(framebufferDescriptor.Label))
        {
            GL.ObjectLabel(GL.ObjectIdentifier.Framebuffer, framebufferId, framebufferName);
        }
        for (var i = 0; i < framebufferDescriptor.ColorAttachments.Length; i++)
        {
            GL.NamedFramebufferTexture(
                framebufferId,
                (GL.FramebufferAttachment)((int)GL.FramebufferAttachment.ColorAttachment0 + i),
                framebufferDescriptor.ColorAttachments[i].Texture.Id,
                0);
            drawBuffers.Add((GL.DrawBuffer)((int)GL.DrawBuffer.ColorAttachment0 + i));
        }

        if (drawBuffers.Any())
        {
            GL.NamedFramebufferDrawBuffers(
                framebufferId,
                drawBuffers.ToArray());
        }
        else
        {
            GL.NamedFramebufferDrawBuffers(framebufferId, GL.DrawBuffer.None);
        }

        if (framebufferDescriptor.DepthAttachment.HasValue &&
            framebufferDescriptor.StencilAttachment.HasValue)
        {
            GL.NamedFramebufferTexture(
                framebufferId,
                GL.FramebufferAttachment.DepthStencilAttachment,
                framebufferDescriptor.DepthAttachment.Value.Texture.Id,
                0);
        }
        else if (framebufferDescriptor.DepthAttachment.HasValue)
        {
            GL.NamedFramebufferTexture(
                framebufferId,
                GL.FramebufferAttachment.DepthAttachment,
                framebufferDescriptor.DepthAttachment.Value.Texture.Id,
                0);
        }
        else if (framebufferDescriptor.StencilAttachment.HasValue)
        {
            GL.NamedFramebufferTexture(
                framebufferId,
                GL.FramebufferAttachment.StencilAttachment,
                framebufferDescriptor.StencilAttachment.Value.Texture.Id,
                0);
        }

        _framebufferCache.Add(framebufferDescriptor, framebufferId);
        _logger.Debug("{Category}: Framebuffer {FramebufferName} created", "FramebufferCache", framebufferName);

        return framebufferId;
    }
}