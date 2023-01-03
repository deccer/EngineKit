using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace EngineKit.Graphics;

internal sealed class ImageLibrary : IImageLibrary
{
    private readonly ILogger _logger;
    private readonly IImageLoader _imageLoader;
    private readonly IDictionary<string, string> _imageFilePathsToBeLoaded;
    private readonly IDictionary<string, byte[]> _imagesToBeLoaded;
    private readonly IList<string> _loadedImages;

    public bool FlipHorizontal { get; set; }

    public bool FlipVertical { get; set; }

    public ImageLibrary(
        ILogger logger,
        IImageLoader imageLoader)
    {
        _logger = logger;
        _imageLoader = imageLoader;
        _imageFilePathsToBeLoaded = new Dictionary<string, string>();
        _imagesToBeLoaded = new Dictionary<string, byte[]>();
        _loadedImages = new List<string>();
        FlipVertical = true;
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

    public IDictionary<string, IList<ImageLibraryItem>> GetImageDataPerMaterial(IReadOnlyList<Material> materials)
    {
        //TODO(deccer) this is a hack
        _loadedImages.Clear();

        var imagesPerMaterial = new Dictionary<string, IList<ImageLibraryItem>>();

        foreach (var material in materials)
        {
            LoadBaseColorData(material, imagesPerMaterial);
            LoadNormalData(material, imagesPerMaterial);
            LoadSpecularData(material, imagesPerMaterial);
            LoadMetalnessRoughnessData(material, imagesPerMaterial);
        }

        return imagesPerMaterial;
    }

    private void LoadBaseColorData(Material material, IDictionary<string, IList<ImageLibraryItem>> imagesPerMaterial)
    {
        if (!string.IsNullOrEmpty(material.BaseColorTextureDataName) && _imageFilePathsToBeLoaded.TryGetValue(material.BaseColorTextureDataName, out var baseColorTextureFilePath))
        {
            if (File.Exists(baseColorTextureFilePath))
            {
                LoadImageFromFile( material.Name, material.BaseColorTextureDataName, baseColorTextureFilePath, imagesPerMaterial);
            }
            else
            {
                _logger.Warning("{Category}: {FilePath} not found. Loading fallback 'Data/Default/T_Default_B.png", nameof(ImageLibrary), baseColorTextureFilePath);
                LoadImageFromFile(material.Name, "T_Default_B", "Data/Default/T_Default_B.png", imagesPerMaterial);
            }
        }

        if (!string.IsNullOrEmpty(material.BaseColorTextureDataName) && _imagesToBeLoaded.TryGetValue(material.BaseColorTextureDataName, out var baseColorImageSpan))
        {
            LoadImageFromSpan(material.Name, material.BaseColorTextureDataName, baseColorImageSpan, imagesPerMaterial);
        }
    }

    private void LoadNormalData(Material material, IDictionary<string, IList<ImageLibraryItem>> imagesPerMaterial)
    {
        if (!string.IsNullOrEmpty(material.NormalTextureDataName) && _imageFilePathsToBeLoaded.TryGetValue(material.NormalTextureDataName, out var normalTextureFilePath))
        {
            if (File.Exists(normalTextureFilePath))
            {
                LoadImageFromFile(material.Name, material.NormalTextureDataName, normalTextureFilePath, imagesPerMaterial);
            }
            else
            {
                _logger.Warning("{Category}: {FilePath} not found. Loading fallback 'Data/Default/T_Default_N.png", nameof(ImageLibrary), normalTextureFilePath);
                LoadImageFromFile(material.Name, "T_Default_N", "Data/Default/T_Default_N.png", imagesPerMaterial);
            }
        }

        if (!string.IsNullOrEmpty(material.NormalTextureDataName) && _imagesToBeLoaded.TryGetValue(material.NormalTextureDataName, out var normalImageSpan))
        {
            LoadImageFromSpan(material.Name, material.NormalTextureDataName, normalImageSpan, imagesPerMaterial);
        }
    }

    private void LoadSpecularData(Material material, IDictionary<string, IList<ImageLibraryItem>> imagesPerMaterial)
    {
        if (!string.IsNullOrEmpty(material.SpecularTextureDataName) && _imageFilePathsToBeLoaded.TryGetValue(material.SpecularTextureDataName, out var specularTextureFilePath))
        {
            if (File.Exists(specularTextureFilePath))
            {
                LoadImageFromFile(material.Name, material.SpecularTextureDataName, specularTextureFilePath, imagesPerMaterial);
            }
            else
            {
                _logger.Warning("{Category}: {FilePath} not found. Loading fallback 'Data/Default/T_Default_S.png", nameof(ImageLibrary), specularTextureFilePath);
                LoadImageFromFile(material.Name, "T_Default_S", "Data/Default/T_Default_S.png", imagesPerMaterial);
            }
        }

        if (!string.IsNullOrEmpty(material.SpecularTextureDataName) && _imagesToBeLoaded.TryGetValue(material.SpecularTextureDataName, out var specularImageSpan))
        {
            LoadImageFromSpan(material.Name, material.SpecularTextureDataName, specularImageSpan, imagesPerMaterial);
        }
    }

    private void LoadMetalnessRoughnessData(Material material, IDictionary<string, IList<ImageLibraryItem>> imagesPerMaterial)
    {
        if (!string.IsNullOrEmpty(material.MetalnessRoughnessTextureDataName) && _imageFilePathsToBeLoaded.TryGetValue(material.MetalnessRoughnessTextureDataName, out var roughnessTextureFilePath))
        {
            if (File.Exists(roughnessTextureFilePath))
            {
                LoadImageFromFile(material.Name, material.MetalnessRoughnessTextureDataName, roughnessTextureFilePath, imagesPerMaterial);
            }
            else
            {
                _logger.Warning("{Category}: {FilePath} not found. Loading fallback 'Data/Default/T_Default_MR.png", nameof(ImageLibrary), roughnessTextureFilePath);
                LoadImageFromFile(material.Name, "T_Default_MR", "Data/Default/T_Default_MR.png", imagesPerMaterial);
            }
        }

        if (!string.IsNullOrEmpty(material.MetalnessRoughnessTextureDataName) && _imagesToBeLoaded.TryGetValue(material.MetalnessRoughnessTextureDataName, out var roughnessImageSpan))
        {
            LoadImageFromSpan(material.Name, material.MetalnessRoughnessTextureDataName, roughnessImageSpan, imagesPerMaterial);
        }
    }

    private void LoadImageFromSpan(
        string materialName,
        string imageName,
        ReadOnlySpan<byte> imageSpan,
        IDictionary<string, IList<ImageLibraryItem>> imagesPerMaterial)
    {
        if (!_loadedImages.Contains(imageName))
        {
            var stopwatch = Stopwatch.StartNew();
            var image = Image.Load<Rgba32>(imageSpan);

            LoadImage(
                materialName,
                imageName,
                null,
                image,
                imagesPerMaterial);
            stopwatch.Stop();
            _logger.Debug(
                "{Category}: Loading image {ImageName} for material {MaterialName} from memory. Took {LoadingTime} ms",
                "ImageLibrary", imageName, materialName, stopwatch.ElapsedMilliseconds);
            _loadedImages.Add(imageName);
        }
    }

    private void LoadImageFromFile(
        string materialName,
        string imageName,
        string imageFilePath,
        IDictionary<string, IList<ImageLibraryItem>> imagesPerMaterial)
    {
        if (!_loadedImages.Contains(imageName))
        {
            var stopwatch = Stopwatch.StartNew();
            var image = _imageLoader.LoadImage<Rgba32>(imageFilePath);

            LoadImage(
                materialName,
                imageName,
                imageFilePath,
                image,
                imagesPerMaterial);
            stopwatch.Stop();

            _logger.Debug("{Category}: Loading image from {FilePath}. Took {LoadingTime} ms", "ImageLibrary",
                imageFilePath, stopwatch.ElapsedMilliseconds);
            _loadedImages.Add(imageName);
        }
    }

    private void LoadImage(
        string materialName,
        string imageName,
        string? imageFilePath,
        Image? image,
        IDictionary<string, IList<ImageLibraryItem>> imagesPerMaterial)
    {
        if (FlipVertical)
        {
            image.Mutate(i => i.Flip(FlipMode.Vertical));
        }

        if (FlipHorizontal)
        {
            image.Mutate(i => i.Flip(FlipMode.Horizontal));
        }
        var textureArrayIndex = (int)Math.Log2(image!.Width);
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