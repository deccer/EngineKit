using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace EngineKit.Graphics;

public interface IImageLoader
{
    Image? LoadImageFromFile<TPixel>(string filePath, bool flipVertical = true, bool flipHorizontal = false) where TPixel: IPixel;

    Image? LoadImageFromMemory<TPixel>(ReadOnlySpan<byte> pixelBytes, bool flipVertical = true, bool flipHorizontal = false) where TPixel : IPixel;
}