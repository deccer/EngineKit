namespace EngineKit.Graphics;

public record struct ClearValue
{
    public static ClearValue Zero = new ClearValue(0.0f, 0.0f, 0.0f, 1.0f, 1.0f, 0);
    public static ClearValue White = new ClearValue(1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0);
    public static ClearValue DarkGray = new ClearValue(0.1f, 0.1f, 0.1f, 1.0f, 1.0f, 0);

    public ClearValue(float x, float y, float z, float w, float depth = 1.0f, int stencil = 0)
    {
        Color = new ClearColorValue
        {
            ColorFloat = new[] { x, y, z, w }
        };

        DepthStencil = new ClearDepthStencilValue(depth, stencil);
    }

    public ClearValue(int x, int y, int z, int w, float depth = 1.0f, int stencil = 0)
    {
        Color = new ClearColorValue
        {
            ColorSignedInteger = new[] { x, y, z, w }
        };

        DepthStencil = new ClearDepthStencilValue(depth, stencil);
    }

    public ClearValue(uint x, uint y, uint z, uint w, float depth = 1.0f, int stencil = 0)
    {
        Color = new ClearColorValue
        {
            ColorUnsignedInteger = new[] { x, y, z, w }
        };

        DepthStencil = new ClearDepthStencilValue(depth, stencil);
    }

    public ClearColorValue Color { get; }

    public ClearDepthStencilValue DepthStencil { get; }
}