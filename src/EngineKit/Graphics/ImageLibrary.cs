using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace EngineKit.Graphics;

internal sealed class ImageLibrary : IImageLibrary
{
    private readonly ILogger _logger;
    private readonly IDictionary<string, string> _imageFilePathsToBeLoaded;
    private readonly IDictionary<string, byte[]> _imagesToBeLoaded;

    public ImageLibrary(ILogger logger)
    {
        _logger = logger;
        _imageFilePathsToBeLoaded = new Dictionary<string, string>();
        _imagesToBeLoaded = new Dictionary<string, byte[]>();
    }

    public void AddImage(string name, string filePath)
    {
        if (_imageFilePathsToBeLoaded.ContainsKey(name))
        {
            _logger.Information("{Category}: Image {ImageName} already registered to be loaded", nameof(ImageLibrary), name);
            return;
        }

        _imageFilePathsToBeLoaded.Add(name, filePath);
    }

    public void AddImage(string name, ReadOnlySpan<byte> imageSpan)
    {
        if (_imagesToBeLoaded.ContainsKey(name))
        {
            _logger.Information("{Category}: Image {ImageName} already registered to be loaded", nameof(ImageLibrary), name);
            return;
        }

        _imagesToBeLoaded.Add(name, imageSpan.ToArray());
    }

    public IDictionary<string, IList<ImageLibraryItem>> GetImageDataPerMaterial(IImmutableList<Material> materials)
    {
        var imagesPerMaterial = new Dictionary<string, IList<ImageLibraryItem>>();

        foreach (var material in materials)
        {
            if (!string.IsNullOrEmpty(material.BaseColorTextureDataName) && _imageFilePathsToBeLoaded.TryGetValue(material.BaseColorTextureDataName, out var baseColorTextureFilePath))
            {
                if (File.Exists(baseColorTextureFilePath))
                {
                    LoadImageFromFile( material.Name, material.BaseColorTextureDataName, baseColorTextureFilePath, imagesPerMaterial);
                }
                else
                {
                    //TODO use dummy texture
                    _logger.Warning("{Category}: {FilePath} not found", nameof(ImageLibrary), baseColorTextureFilePath);
                }
            }

            if (!string.IsNullOrEmpty(material.BaseColorTextureDataName) && _imagesToBeLoaded.TryGetValue(material.BaseColorTextureDataName, out var baseColorImageSpan))
            {
                LoadImageFromSpan(material.Name, material.BaseColorTextureDataName, baseColorImageSpan, imagesPerMaterial);
            }

            if (!string.IsNullOrEmpty(material.NormalTextureDataName) && _imageFilePathsToBeLoaded.TryGetValue(material.NormalTextureDataName, out var normalTextureFilePath))
            {
                if (File.Exists(normalTextureFilePath))
                {
                    LoadImageFromFile(material.Name, material.NormalTextureDataName, normalTextureFilePath, imagesPerMaterial);
                }
                else
                {
                    //TODO use dummy texture
                    _logger.Warning("{Category}: {FilePath} not found", nameof(ImageLibrary), normalTextureFilePath);
                }
            }

            if (!string.IsNullOrEmpty(material.NormalTextureDataName) && _imagesToBeLoaded.TryGetValue(material.NormalTextureDataName, out var normalImageSpan))
            {
                LoadImageFromSpan(material.Name, material.NormalTextureDataName, normalImageSpan, imagesPerMaterial);
            }
        }

        return imagesPerMaterial;
    }

    private static void LoadImageFromSpan(
        string materialName,
        string imageName,
        ReadOnlySpan<byte> imageSpan,
        IDictionary<string, IList<ImageLibraryItem>> imagesPerMaterial)
    {
        var image = Image.Load<Rgba32>(imageSpan);

        LoadImage(
            materialName,
            imageName,
            null,
            image,
            imagesPerMaterial);
    }

    private static void LoadImageFromFile(
        string materialName,
        string imageName,
        string imageFilePath,
        IDictionary<string, IList<ImageLibraryItem>> imagesPerMaterial)
    {
        var image = Image.Load<Rgba32>(imageFilePath);

        LoadImage(
            materialName,
            imageName,
            imageFilePath,
            image,
            imagesPerMaterial);
    }

    private static void LoadImage(
        string materialName,
        string imageName,
        string? imageFilePath,
        Image<Rgba32> image,
        IDictionary<string, IList<ImageLibraryItem>> imagesPerMaterial)
    {
        image.Mutate(i => i.Flip(FlipMode.Vertical));
        var textureArrayIndex = (int)Math.Log2(image.Width);
        var imageLibraryItem = new ImageLibraryItem
        {
            ImageName = imageName,
            ImageFilePath = imageFilePath,
            Image = image,
            TextureArrayIndex = textureArrayIndex
        };

        if (imagesPerMaterial.TryGetValue(materialName, out var imagesPerBucket))
        {
            imagesPerBucket.Add(imageLibraryItem);
        }
        else
        {
            imagesPerBucket = new List<ImageLibraryItem>(128);
            imagesPerBucket.Add(imageLibraryItem);
            imagesPerMaterial.Add(materialName, imagesPerBucket);
        }
    }
}