using System;
using System.Collections.Generic;
using System.Diagnostics;
using EngineKit.Mathematics;
using Serilog;
using Vector3 = System.Numerics.Vector3;

namespace EngineKit.Graphics;

public record Material(string Name) : IDisposable
{
    public const string MaterialNotFoundName = "M_NotFound";

    private float _metallicFactor;
    private float _roughnessFactor;
    private Vector3 _specularFactor;
    private float _glossinessFactor;
    private float _occlusionStrength = 1.0f;
    private Color4 _emissiveColor;
    private Color4 _baseColor;
    private Color4 _specularColor;
    private ImageInformation? _baseColorImage;
    private ImageInformation? _normalImage;
    private ImageInformation? _specularImage;
    private ImageInformation? _metalnessRoughnessImage;
    private ImageInformation? _occlusionImage;
    private ImageInformation? _emissiveImage;

    public SamplerInformation? BaseColorTextureSamplerInformation;
    public SamplerInformation? NormalTextureSamplerInformation;
    public SamplerInformation? SpecularTextureSamplerInformation;
    public SamplerInformation? MetalnessRoughnessTextureSamplerInformation;
    public SamplerInformation? OcclusionTextureSamplerInformation;
    public SamplerInformation? EmissiveTextureSamplerInformation;

    private bool _isDirty;
    public string Name { get; set; } = Name;

    public bool TexturesLoaded { get; private set; }

    public bool IsDirty => _isDirty;

    public float OcclusionStrength
    {
        get => _occlusionStrength;
        set
        {
            if (MathF.Abs(_occlusionStrength - value) > 0.0001f)
            {
                _occlusionStrength = value;
                _isDirty = true;
            }
        }
    }

    public float GlossinessFactor
    {
        get => _glossinessFactor;
        set
        {
            if (MathF.Abs(_glossinessFactor - value) > 0.0001f)
            {
                _glossinessFactor = value;
                _isDirty = true;
            }
        }
    }

    public float MetallicFactor
    {
        get => _metallicFactor;
        set
        {
            if (MathF.Abs(_metallicFactor - value) > 0.0001f)
            {
                _metallicFactor = value;
                _isDirty = true;
            }
        }
    }

    public float RoughnessFactor
    {
        get => _roughnessFactor;
        set
        {
            if (MathF.Abs(_roughnessFactor - value) > 0.0001f)
            {
                _roughnessFactor = value;
                _isDirty = true;
            }
        }
    }

    public Color4 BaseColor
    {
        get => _baseColor;
        set
        {
            if (_baseColor != value)
            {
                _baseColor = value;
                _isDirty = true;
            }
        }
    }

    public Color4 EmissiveColor
    {
        get => _emissiveColor;
        set
        {
            if (_emissiveColor != value)
            {
                _emissiveColor = value;
                _isDirty = true;
            }
        }
    }

    public Color4 SpecularColor
    {
        get => _specularColor;
        set
        {
            if (_specularColor != value)
            {
                _specularColor = value;
                _isDirty = true;
            }
        }
    }

    public Vector3 SpecularFactor
    {
        get => _specularFactor;
        set
        {
            _specularFactor = value;
            _isDirty = true;
        }
    }

    public ImageInformation? EmissiveImage
    {
        get => _emissiveImage;
        set
        {
            if (_emissiveImage != value)
            {
                _emissiveImage = value;
                _isDirty = true;
            }
        }
    }

    public ImageInformation? BaseColorImage
    {
        get => _baseColorImage;
        set
        {
            if (_baseColorImage != value)
            {
                _baseColorImage = value;
                _isDirty = true;
            }
        }
    }

    public ImageInformation? NormalImage
    {
        get => _normalImage;
        set
        {
            if (_normalImage != value)
            {
                _normalImage = value;
                _isDirty = true;
            }
        }
    }

    public ImageInformation? SpecularImage
    {
        get => _specularImage;
        set
        {
            if (_specularImage != value)
            {
                _specularImage = value;
                _isDirty = true;
            }
        }
    }

    public ImageInformation? MetalnessRoughnessImage
    {
        get => _metalnessRoughnessImage;
        set
        {
            _metalnessRoughnessImage = value;
            _isDirty = true;
        }
    }

    public ImageInformation? OcclusionImage
    {
        get => _occlusionImage;
        set
        {
            _occlusionImage = value;
            _isDirty = true;
        }
    }

    public ITexture? BaseColorTexture { get; private set; }

    public ITexture? NormalTexture { get; private set; }

    public ITexture? SpecularTexture { get; private set; }

    public ITexture? MetalnessRoughnessTexture { get; private set; }

    public ITexture? OcclusionTexture { get; private set; }

    public ITexture? EmissiveTexture { get; private set; }

    public void Dispose()
    {
        BaseColorTexture?.Dispose();
        NormalTexture?.Dispose();
        SpecularTexture?.Dispose();
        MetalnessRoughnessTexture?.Dispose();
        OcclusionTexture?.Dispose();
        EmissiveTexture?.Dispose();
    }

    public void LoadTextures(
        ILogger logger,
        IGraphicsContext graphicsContext,
        ISamplerLibrary samplerLibrary,
        IDictionary<string, ITexture> textures,
        bool makeResident)
    {
        //TODO(deccer) TextureLoader should be called and pass in material, to set its Texture objects perhaps
        //TODO(deccer) this needs to be inverted, ie someone else needs to load the stuff per material, not the material itself

        BaseColorTexture = CreateTextureFromImage(
            BaseColorImage,
            Format.R8G8B8A8Srgb,
            BaseColorTextureSamplerInformation,
            logger,
            graphicsContext,
            samplerLibrary,
            textures,
            makeResident);
        NormalTexture = CreateTextureFromImage(
            NormalImage,
            Format.R8G8B8A8UNorm,
            NormalTextureSamplerInformation,
            logger,
            graphicsContext,
            samplerLibrary,
            textures,
            makeResident);
        MetalnessRoughnessTexture = CreateTextureFromImage(
            MetalnessRoughnessImage,
            Format.R8G8B8A8UNorm,
            MetalnessRoughnessTextureSamplerInformation,
            logger,
            graphicsContext,
            samplerLibrary,
            textures,
            makeResident);
        SpecularTexture = CreateTextureFromImage(
            SpecularImage,
            Format.R8G8B8A8UNorm,
            SpecularTextureSamplerInformation,
            logger,
            graphicsContext,
            samplerLibrary,
            textures,
            makeResident);
        OcclusionTexture = CreateTextureFromImage(
            OcclusionImage,
            Format.R8G8B8A8UNorm,
            OcclusionTextureSamplerInformation,
            logger,
            graphicsContext,
            samplerLibrary,
            textures,
            makeResident);
        EmissiveTexture = CreateTextureFromImage(
            EmissiveImage,
            Format.R8G8B8A8Srgb,
            EmissiveTextureSamplerInformation,
            logger,
            graphicsContext,
            samplerLibrary,
            textures,
            makeResident);

        TexturesLoaded = true;
    }

    private static ITexture? CreateTextureFromImage(
        ImageInformation? image,
        Format format,
        SamplerInformation? sampler,
        ILogger logger,
        IGraphicsContext graphicsContext,
        ISamplerLibrary samplerLibrary,
        IDictionary<string, ITexture> textures,
        bool makeResident)
    {
        if (image == null ||
            string.IsNullOrEmpty(image.Name) ||
            textures.TryGetValue(image.Name, out var texture))
        {
            return null;
        }

        var sw = Stopwatch.StartNew();
        texture = image.ImageData.HasValue
            ? graphicsContext.CreateTextureFromMemory(image, format, image.Name, generateMipmaps: true, flipVertical: false, flipHorizontal: false)
            : string.IsNullOrEmpty(image.FilePath)
                ? null
                : graphicsContext.CreateTextureFromFile(image.FilePath, format, true, false, false);

        sw.Stop();

        if (texture == null)
        {
            return texture;
        }

        textures.Add(image.Name, texture);
        logger.Debug("{Category}: Loading texture {TextureName} took {LoadingTime}ms", "Material", image.Name, sw.ElapsedMilliseconds);

        if (makeResident)
        {
            if (sampler.HasValue)
            {
                texture.MakeResident(samplerLibrary.GetSampler(sampler.Value));
            }
            else
            {
                texture.MakeResident();
            }
        }

        return texture;
    }
}
