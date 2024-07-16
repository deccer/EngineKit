using System;

namespace EngineKit.Graphics.RHI;

[Flags]
public enum FramebufferBit
{
    DepthBufferBit = 1,
    StencilBufferBit = 2,
    ColorBufferBit = 4,
}