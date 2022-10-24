using System.Runtime.CompilerServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace EngineKit.Graphics;

public interface IImageLoader
{
    Image LoadImage<TPixel>(string filePath) where TPixel: IPixel;
}