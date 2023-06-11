using System;
using System.Collections.Generic;
using System.Linq;
using EngineKit.Mathematics;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace EngineKit.Graphics;

internal sealed class TextureLibrary : ITextureLibrary
{
    private readonly IGraphicsContext _graphicsContext;
    private readonly ILogger _logger;

    public TextureLibrary(ILogger logger, IGraphicsContext graphicsContext)
    {
        _graphicsContext = graphicsContext;
        _logger = logger.ForContext<TextureLibrary>();
    }

    public IReadOnlyCollection<ITexture> PrepareTextureArrays(
        IDictionary<string, IList<ImageLibraryItem>> imageDataPerMaterial,
        out IDictionary<string, TextureId> textureArrayIndices)
    {
        var textureArrays = new List<ITexture>();
        textureArrayIndices = new Dictionary<string, TextureId>(256);

        var textureIndices = new Dictionary<int, IList<ImageLibraryItem>>(256);
        foreach (var imageDatePerMaterial in imageDataPerMaterial)
        {
            foreach (var imageLibraryItem in imageDatePerMaterial.Value)
            {
                if (textureIndices.TryGetValue(imageLibraryItem.TextureArrayIndex, out var ilis))
                {
                    var existingIlis = ilis
                        .Where(item => item.ImageName == imageLibraryItem.ImageName)
                        .Where(item => item.TextureArrayIndex == imageLibraryItem.TextureArrayIndex)
                        .Any();
                    if (!existingIlis)
                    {
                        ilis.Add(imageLibraryItem);
                    }
                }
                else
                {
                    ilis = new List<ImageLibraryItem>(256);
                    ilis.Add(imageLibraryItem);
                    textureIndices[imageLibraryItem.TextureArrayIndex] = ilis;
                }
            }
        }

        var textureArrayIndex = 0;
        foreach (var textureIndex in textureIndices)
        {
            var firstImageLibraryItem = textureIndex.Value.First();
            var imageWidth = firstImageLibraryItem.Image!.Width;
            var imageHeight = firstImageLibraryItem.Image!.Height;

            var textureArraySlice = 0;
            ITexture? texture = null;
            foreach (var layer in textureIndex.Value)
            {
                if (textureArraySlice == 0)
                {
                    var textureCreateDescriptor = new TextureCreateDescriptor
                    {
                        ImageType = ImageType.Texture2DArray,
                        Format = Format.R8G8B8A8UNorm,
                        Label = $"TA_{textureIndex.Key}_{imageWidth}x{imageHeight}x{textureIndex.Value.Count}",
                        Size = new Int3(imageWidth, imageHeight, 1),
                        ArrayLayers = (uint)textureIndex.Value.Count,
                        MipLevels = 1 + (uint)MathF.Ceiling(MathF.Log2(MathF.Max(imageWidth, imageHeight))),
                        SampleCount = SampleCount.OneSample
                    };

                    texture = _graphicsContext.CreateTexture(textureCreateDescriptor);
                }

                var textureUploadDescriptor = new TextureUpdateDescriptor
                {
                    Level = 0,
                    Offset = new Int3(0, 0, textureArraySlice),
                    Size = new Int3(imageWidth, imageHeight, 1),
                    UploadDimension = UploadDimension.Three,
                    UploadFormat = UploadFormat.RedGreenBlueAlpha,
                    UploadType = UploadType.UnsignedByte
                };

                var image = layer.Image;
                switch (image)
                {
                    case Image<Rgb24> imageRgb24 when
                        imageRgb24.DangerousTryGetSinglePixelMemory(out var pixelSpan24):
                        texture!.Update(textureUploadDescriptor, pixelSpan24.Pin());
                        break;
                    case Image<Rgba32> imageRgba32 when
                        imageRgba32.DangerousTryGetSinglePixelMemory(out var pixelSpan32):
                        texture!.Update(textureUploadDescriptor, pixelSpan32.Pin());
                        break;
                }
                texture!.GenerateMipmaps();

                if (!textureArrayIndices.ContainsKey(layer.ImageName))
                {
                    textureArrayIndices.Add(layer.ImageName, new TextureId(textureArrayIndex, textureArraySlice));
                }

                textureArraySlice++;
            }

            if (texture != null)
            {
                textureArrays.Add(texture);
            }

            textureArrayIndex++;
        }

        return textureArrays;
    }
}