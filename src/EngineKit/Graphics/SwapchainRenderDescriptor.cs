using OpenTK.Mathematics;

namespace EngineKit.Graphics;

public record struct SwapchainRenderDescriptor
{
    public Vector4i Viewport;

    public Vector4i? ScissorRect;

    public bool ClearColor;

    public ClearColorValue ClearColorValue;

    public bool ClearDepth;

    public float ClearDepthValue;

    public bool ClearStencil;

    public int ClearStencilValue;
}