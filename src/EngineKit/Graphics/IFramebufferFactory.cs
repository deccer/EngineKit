using System;

namespace EngineKit.Graphics;

internal interface IFramebufferFactory : IDisposable
{
    uint GetOrCreateFramebuffer(FramebufferRenderDescriptor framebufferRenderDescriptor);
}