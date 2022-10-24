using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace EngineKit.Graphics;

internal sealed class SixLaborsImageLoader : IImageLoader
{
    public Image LoadImage<TPixel>(string filePath) where TPixel : IPixel
    {
        if (typeof(TPixel) == typeof(Rgb24))
        {
            return Image.Load<Rgb24>(filePath);
        };
        
        if (typeof(TPixel) == typeof(Rgba32))
        {
            return Image.Load<Rgba32>(filePath);
        };

        throw new ArgumentException("Unsupported Pixel type");
    }
}