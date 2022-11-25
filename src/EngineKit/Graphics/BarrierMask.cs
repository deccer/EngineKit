using System;

namespace EngineKit.Graphics;

[Flags]
public enum BarrierMask : uint
{
    VertexAttribArray = 1,
    ElementArray = 2,
    Uniform = 4,
    TextureFetch = 8,
    ShaderGlobalAccess = 16,
    ShaderImageAccess = 32,
    Command = 64,
    PixelBuffer = 128,
    TextureUpdate = 256,
    BufferUpdate = 512,
    Framebuffer = 1024,
    TransformFeedback = 2048,
    AtomicCounter = 4096,
    ShaderStorage = 8192,
    ClientMappedBuffer = 16384,
    QueryBuffer = 32768,
    All = 4294967295,
}