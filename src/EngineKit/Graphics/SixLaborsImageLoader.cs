using System;
using System.IO;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace EngineKit.Graphics;

internal sealed class SixLaborsImageLoader : IImageLoader
{
    private readonly ILogger _logger;

    public SixLaborsImageLoader(ILogger logger)
    {
        _logger = logger.ForContext<SixLaborsImageLoader>();
    }
    
    public Image? LoadImage<TPixel>(string filePath) where TPixel : IPixel
    {
        if (!File.Exists(filePath))
        {
            _logger.Error("ImageLoader: File {FilePath} not found", filePath);
            return null;
        }
        
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