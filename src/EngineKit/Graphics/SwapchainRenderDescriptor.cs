using EngineKit.Mathematics;

namespace EngineKit.Graphics;

public record struct SwapchainRenderDescriptor
{
    public Viewport Viewport;

    public Viewport? ScissorRect;

    public bool ClearColor;

    public ClearColorValue ClearColorValue;

    public bool ClearDepth;

    public float ClearDepthValue;

    public bool ClearStencil;

    public int ClearStencilValue;
}