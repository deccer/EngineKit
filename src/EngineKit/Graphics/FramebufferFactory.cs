using System.Collections.Generic;
using System.Linq;
using EngineKit.Native.OpenGL;
using Serilog;

namespace EngineKit.Graphics;

internal sealed class FramebufferFactory : IFramebufferFactory
{
    private readonly ILogger _logger;
    private readonly IDictionary<FramebufferRenderDescriptor, uint> _framebufferCache;

    public FramebufferFactory(ILogger logger)
    {
        _logger = logger.ForContext<FramebufferFactory>();
        _framebufferCache = new Dictionary<FramebufferRenderDescriptor, uint>(16);
    }

    public void Dispose()
    {
        foreach (var fbo in _framebufferCache.Values)
        {
            GL.DeleteFramebuffer(fbo);
        }

        _framebufferCache.Clear();
    }

    public uint GetOrCreateFramebuffer(FramebufferRenderDescriptor framebufferRenderDescriptor)
    {
        return _framebufferCache.TryGetValue(framebufferRenderDescriptor, out var framebuffer)
            ? framebuffer
            : CreateFramebuffer(framebufferRenderDescriptor);
    }

    public void RemoveFramebuffer(FramebufferRenderDescriptor framebufferRenderDescriptor)
    {
        if (_framebufferCache.TryGetValue(framebufferRenderDescriptor, out var framebuffer))
        {
            GL.DeleteFramebuffer(framebuffer);
            _framebufferCache.Remove(framebufferRenderDescriptor);
        }
    }

    private uint CreateFramebuffer(FramebufferRenderDescriptor framebufferRenderDescriptor)
    {
        _logger.Debug("Creating Framebuffer");

        var drawBuffers = new List<GL.DrawBuffer>(8);
        var framebufferId = GL.CreateFramebuffer();
        for (var i = 0; i < framebufferRenderDescriptor.ColorAttachments.Length; i++)
        {
            GL.NamedFramebufferTexture(
                framebufferId,
                (GL.FramebufferAttachment)((int)GL.FramebufferAttachment.ColorAttachment0 + i),
                framebufferRenderDescriptor.ColorAttachments[i].Texture.Id,
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

        if (framebufferRenderDescriptor.DepthAttachment.HasValue &&
            framebufferRenderDescriptor.StencilAttachment.HasValue)
        {
            GL.NamedFramebufferTexture(
                framebufferId,
                GL.FramebufferAttachment.DepthStencilAttachment,
                framebufferRenderDescriptor.DepthAttachment.Value.Texture.Id,
                0);
        }
        else if (framebufferRenderDescriptor.DepthAttachment.HasValue)
        {
            GL.NamedFramebufferTexture(
                framebufferId,
                GL.FramebufferAttachment.DepthAttachment,
                framebufferRenderDescriptor.DepthAttachment.Value.Texture.Id,
                0);
        }
        else if (framebufferRenderDescriptor.StencilAttachment.HasValue)
        {
            GL.NamedFramebufferTexture(
                framebufferId,
                GL.FramebufferAttachment.StencilAttachment,
                framebufferRenderDescriptor.StencilAttachment.Value.Texture.Id,
                0);
        }

        _framebufferCache.Add(framebufferRenderDescriptor, framebufferId);

        return framebufferId;
    }
}