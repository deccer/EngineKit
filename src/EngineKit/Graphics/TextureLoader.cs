using System;
using System.IO;
using EngineKit.Mathematics;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace EngineKit.Graphics;

internal sealed class TextureLoader : ITextureLoader
{
    private readonly IGraphicsContext _graphicsContext;
    private readonly ILogger _logger;

    public TextureLoader(
        ILogger logger,
        IGraphicsContext graphicsContext)
    {
        _graphicsContext = graphicsContext;
        _logger = logger.ForContext<TextureLoader>();
    }

    public ITexture? LoadTextureFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            _logger.Error("{Category}: File {FilePath} does not exist", nameof(TextureLibrary), filePath);
            return null;
        }

        Configuration.Default.PreferContiguousImageBuffers = true;

        var image = Image.Load<Rgba32>(filePath);
        image.Mutate(ipc => ipc.Flip(FlipMode.Vertical));

        var imageWidth = image.Width;
        var imageHeight = image.Height;
        var mipLevels = (int)MathF.Floor(MathF.Log2(MathF.Max(imageWidth, imageHeight)));

        var textureCreateDescriptor = new TextureCreateDescriptor
        {
            Format = Format.R8G8B8A8UNorm,
            Size = new Int3(image.Width, image.Height, 1),
            Label = $"T_{Path.GetFileName(filePath)}",
            ArrayLayers = 0,
            ImageType = ImageType.Texture2D,
            MipLevels = 1,
            SampleCount = SampleCount.OneSample
        };
        var texture = _graphicsContext.CreateTexture(textureCreateDescriptor);
        UploadImage(image, texture);

        return texture;
    }

    private void UploadImage(Image<Rgba32> image, ITexture texture)
    {
        var textureUpdateDescriptor = new TextureUpdateDescriptor
        {
            Level = 0,
            Offset = Int3.Zero,
            Size = new Int3(image.Width, image.Height, 1),
            UploadDimension = UploadDimension.Two,
            UploadFormat = UploadFormat.RedGreenBlueAlpha,
            UploadType = UploadType.UnsignedByte
        };

        if (!image.DangerousTryGetSinglePixelMemory(out var pixelMemory))
        {
            _logger.Debug("{Category}: Unable to grab memory", nameof(TextureLoader));
            return;
        }

        texture.Update(textureUpdateDescriptor, pixelMemory.Pin());
    }
}