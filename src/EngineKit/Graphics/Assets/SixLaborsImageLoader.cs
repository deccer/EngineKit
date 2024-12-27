using System;
using System.IO;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace EngineKit.Graphics.Assets;

internal sealed class SixLaborsImageLoader : IImageLoader
{
    private readonly ILogger _logger;

    public SixLaborsImageLoader(ILogger logger)
    {
        _logger = logger.ForContext<SixLaborsImageLoader>();
    }

    public Image? LoadImageFromFile<TPixel>(string filePath, bool flipVertical = true, bool flipHorizontal = false) where TPixel : IPixel
    {
        if (!File.Exists(filePath))
        {
            _logger.Error("ImageLoader: File {FilePath} not found", filePath);
            return null;
        }

        if (typeof(TPixel) == typeof(Rgb24))
        {
            var image = Image.Load<Rgb24>(filePath);
            if (flipVertical)
            {
                image.Mutate(pc => pc.Flip(FlipMode.Vertical));
            }

            if (flipHorizontal)
            {
                image.Mutate(pc => pc.Flip(FlipMode.Horizontal));
            }

            return image;
        };

        if (typeof(TPixel) == typeof(Rgba32))
        {
            var image = Image.Load<Rgba32>(filePath);
            if (flipVertical)
            {
                image.Mutate(pc => pc.Flip(FlipMode.Vertical));
            }

            if (flipHorizontal)
            {
                image.Mutate(pc => pc.Flip(FlipMode.Horizontal));
            }

            return image;
        };

        throw new ArgumentException("Unsupported Pixel type");
    }

    public Image? LoadImageFromMemory<TPixel>(ReadOnlySpan<byte> pixelBytes, bool flipVertical = true, bool flipHorizontal = false) where TPixel : IPixel
    {
        if (pixelBytes.IsEmpty)
        {
            _logger.Error("ImageLoader: pixelBytes is empty");
        }
        if (typeof(TPixel) == typeof(Rgb24))
        {
            var image = Image.Load<Rgb24>(pixelBytes);
            if (flipVertical)
            {
                image.Mutate(pc => pc.Flip(FlipMode.Vertical));
            }

            if (flipHorizontal)
            {
                image.Mutate(pc => pc.Flip(FlipMode.Horizontal));
            }

            return image;
        };

        if (typeof(TPixel) == typeof(Rgba32))
        {
            var image = Image.Load<Rgba32>(pixelBytes);
            if (flipVertical)
            {
                image.Mutate(pc => pc.Flip(FlipMode.Vertical));
            }

            if (flipHorizontal)
            {
                image.Mutate(pc => pc.Flip(FlipMode.Horizontal));
            }

            return image;
        };

        throw new ArgumentException("Unsupported Pixel type");
    }
}