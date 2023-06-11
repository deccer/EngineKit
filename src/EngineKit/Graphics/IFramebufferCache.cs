using System;

namespace EngineKit.Graphics;

internal interface IFramebufferCache : IDisposable
{
    uint GetOrCreateFramebuffer(FramebufferDescriptor framebufferDescriptor);

    void RemoveFramebuffer(FramebufferDescriptor framebufferDescriptor);
}