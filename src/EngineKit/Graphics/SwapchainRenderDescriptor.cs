using EngineKit.Mathematics;

namespace EngineKit.Graphics;

public record struct SwapchainRenderDescriptor
{
    public Int4 Viewport;

    public Int4? ScissorRect;

    public bool ClearColor;

    public ClearColorValue ClearColorValue;

    public bool ClearDepth;

    public float ClearDepthValue;

    public bool ClearStencil;

    public int ClearStencilValue;
}