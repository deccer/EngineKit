using System;

namespace EngineKit.Graphics;

internal interface IFramebufferCache : IDisposable
{
    uint GetOrCreateFramebuffer(FramebufferRenderDescriptor framebufferRenderDescriptor);

    void RemoveFramebuffer(FramebufferRenderDescriptor framebufferRenderDescriptor);
}