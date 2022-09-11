using System.Collections.Generic;
using System.Linq;
using EngineKit.Mathematics;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace EngineKit.Graphics;

internal sealed class TextureLibrary : ITextureLibrary
{
    private readonly ILogger _logger;
    private readonly ITextureLoader _textureLoader;
    private readonly IDictionary<string, ITexture> _textures;
    private readonly IDictionary<string, string> _texturesToBeLoaded;
    private readonly IDictionary<string, ulong> _textureResidentHandles;

    public TextureLibrary(
        ILogger logger,
        ITextureLoader textureLoader)
    {
        _logger = logger.ForContext<TextureLibrary>();
        _textureLoader = textureLoader;
        _textures = new Dictionary<string, ITexture>(256);
        _texturesToBeLoaded = new Dictionary<string, string>(256);
        _textureResidentHandles = new Dictionary<string, ulong>(256);
    }

    public void AddTexture(string name, string fileName)
    {
        if (_texturesToBeLoaded.ContainsKey(name))
        {
            _logger.Debug("{Category}: Texture {TextureName} already known", nameof(TextureLibrary), name);
            return;
        }

        _texturesToBeLoaded.Add(name, fileName);
    }

    public void AddTexture(string name, Image image)
    {
    }

    public ulong GetTextureHandle(string textureName)
    {
        return _textureResidentHandles.TryGetValue(textureName, out var textureHandle)
            ? textureHandle
            : 0;
    }

    public void LoadTextures()
    {
        foreach (var (textureName, textureFilePath) in _texturesToBeLoaded)
        {
            if (_textures.ContainsKey(textureName))
            {
                _logger.Debug("{Category}: Texture {TextureName} already loaded", nameof(TextureLibrary), textureName);
                continue;
            }

            var texture = _textureLoader.LoadTextureFromFile(textureFilePath);
            //var textureResidentHandle = texture.MakeResident();
            //_textureResidentHandle.Add(textureName, textureResidentHandle);
            if (texture != null)
            {
                _textures.Add(textureName, texture);
            }
        }
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

        var minTextureArrayIndex = textureIndices.Keys.Min();

        for (var textureArrayIndex = minTextureArrayIndex; textureArrayIndex < minTextureArrayIndex + textureIndices.Count; textureArrayIndex++)
        {
            if (!textureIndices.ContainsKey(textureArrayIndex))
            {
                continue;
            }

            var imageLibraryItems = textureIndices[textureArrayIndex];

            var firstImageLibraryItem = imageLibraryItems.FirstOrDefault();

            var textureArraySlice = 0;
            Texture? texture = null;
            foreach (var layer in imageLibraryItems)
            {
                if (textureArraySlice == 0)
                {
                    var textureCreateDescriptor = new TextureCreateDescriptor
                    {
                        ImageType = ImageType.Texture2DArray,
                        Format = Format.R8G8B8A8UNorm,
                        Label = $"TA_{textureArrayIndex}",
                        Size = new Int3(firstImageLibraryItem.Image.Width,
                            firstImageLibraryItem.Image.Height, 1),
                        ArrayLayers = (uint)imageLibraryItems.Count,
                        MipLevels = 1,
                        SampleCount = SampleCount.OneSample
                    };

                    texture = new Texture(textureCreateDescriptor);
                }

                var textureUploadDescriptor = new TextureUpdateDescriptor
                {
                    Level = 0,
                    Offset = new Int3(0, 0, textureArraySlice),
                    Size = new Int3(firstImageLibraryItem.Image.Width,
                        firstImageLibraryItem.Image.Height, 1),
                    UploadDimension = UploadDimension.Three,
                    UploadFormat = UploadFormat.RedGreenBlueAlpha,
                    UploadType = UploadType.UnsignedByte
                };

                var image32 = layer.Image.CloneAs<Rgba32>();
                if (image32.DangerousTryGetSinglePixelMemory(out var pixelSpan))
                {
                    texture.Upload(textureUploadDescriptor, pixelSpan.Pin());
                }

                if (!textureArrayIndices.TryGetValue(layer.ImageName, out var textureId))
                {
                    textureArrayIndices.Add(layer.ImageName, new TextureId(textureArrayIndex - minTextureArrayIndex, textureArraySlice));
                }
                else
                {
                    if (textureId.ArrayIndex != (textureArrayIndex - minTextureArrayIndex) || textureId.ArraySlice != textureArraySlice)
                    {
                        _logger.Error("Noooooo");
                    }
                }

                textureArraySlice++;
            }

            if (texture != null)
            {
                textureArrays.Add(texture);
            }
        }

        return textureArrays;
    }
}