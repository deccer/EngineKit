using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace EngineKit.Graphics;

public struct ImageLibraryItem
{
    public string ImageName { get; set; }

    public string? ImageFilePath { get; set; }

    public Image<Rgba32> Image { get; set; }

    public int TextureArrayIndex { get; set; }
}