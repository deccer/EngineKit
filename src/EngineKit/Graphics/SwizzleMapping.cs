using EngineKit.Graphics.RHI;

namespace EngineKit.Graphics;

public readonly record struct SwizzleMapping
{
    public static SwizzleMapping CreateForDepthTextures()
    {
        return new SwizzleMapping(Swizzle.Red, Swizzle.Red, Swizzle.Red, Swizzle.One);
    }
    
    public SwizzleMapping(
        Swizzle red = Swizzle.Red,
        Swizzle green = Swizzle.Green,
        Swizzle blue = Swizzle.Blue,
        Swizzle alpha = Swizzle.Alpha)
    {
        Red = red;
        Green = green;
        Blue = blue;
        Alpha = alpha;
    }
    
    public readonly Swizzle Red;
    
    public readonly Swizzle Green;
    
    public readonly Swizzle Blue;
    
    public readonly Swizzle Alpha;
}