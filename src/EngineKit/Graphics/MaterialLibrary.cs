using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using Serilog;

namespace EngineKit.Graphics;

internal sealed class MaterialLibrary : IMaterialLibrary
{
    private readonly ILogger _logger;
    private readonly ITextureLibrary _textureLibrary;
    private readonly ITextureLoader _textureLoader;
    private readonly IImageLibrary _imageLibrary;
    private readonly IDictionary<string, Material> _materials;
    private readonly Random _random;

    public MaterialLibrary(
        ILogger logger,
        ITextureLibrary textureLibrary,
        ITextureLoader textureLoader,
        IImageLibrary imageLibrary)
    {
        _logger = logger.ForContext<MaterialLibrary>();
        _textureLibrary = textureLibrary;
        _textureLoader = textureLoader;
        _imageLibrary = imageLibrary;
        _materials = new Dictionary<string, Material>(256);
        _random = new Random();
        CreateDefaultMaterials();
    }

    private void CreateDefaultMaterials()
    {
        var colors = typeof(Color4)
            .GetFields()
            .Where(field => field.IsStatic && field.IsPublic);

        foreach (var color in colors)
        {
            var materialName = $"M_Default_{color.Name}";
            var colorValue = (Color4)color.GetValue(color);
            _materials.Add(materialName, new Material(materialName)
            {
                BaseColor = colorValue
            });
        }
    }

    public Material GetRandomMaterial()
    {
        return _materials.Values.ElementAt(_random.Next(0, _materials.Values.Count));
    }

    public bool Exists(string materialName)
    {
        return _materials.ContainsKey(materialName);
    }

    public void AddMaterial(string name, Material material)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        //TODO(deccer) error handling re data names can be null etc
        if (_materials.ContainsKey(name))
        {
            _logger.Debug("{Category}: Material {MaterialName} exists already", nameof(MaterialLibrary), name);
            return;
        }

        if (!string.IsNullOrEmpty(material.BaseColorTextureDataName))
        {
            if (material.BaseColorEmbeddedImageData.HasValue)
            {
                var imageSpan = material.BaseColorEmbeddedImageData.Value.Span;
                _imageLibrary.AddImage(material.BaseColorTextureDataName, imageSpan);
            }
            else
            {
                if (!string.IsNullOrEmpty(material.BaseColorTextureFilePath))
                {
                    _imageLibrary.AddImage(material.BaseColorTextureDataName, material.BaseColorTextureFilePath);
                }
            }
        }

        if (!string.IsNullOrEmpty(material.NormalTextureDataName))
        {
            if (material.NormalEmbeddedImageData.HasValue)
            {
                var imageSpan = material.NormalEmbeddedImageData.Value.Span;
                _imageLibrary.AddImage(material.NormalTextureDataName, imageSpan);
            }
            else
            {
                if (!string.IsNullOrEmpty(material.NormalTextureFilePath))
                {
                    _imageLibrary.AddImage(material.NormalTextureDataName, material.NormalTextureFilePath);
                }
            }
        }

        _materials.Add(name, material);
    }

    public IList<GpuMaterial> GetMaterialBufferData(
        string[] visibleMaterialNames,
        IDictionary<string, TextureId> textureArrayIndices,
        out IDictionary<string, int> materialNameIndexMap)
    {
        var materials = _materials
            .Where(material => visibleMaterialNames.Contains(material.Key))
            .ToList();

        var materialNames = materials
            .Select(m => m.Key)
            .Distinct()
            .ToList();
        materialNameIndexMap = visibleMaterialNames.ToDictionary(
            visibleMaterialName => visibleMaterialName,
            visibleMaterialName => materialNames.IndexOf(visibleMaterialName));

        return materials
            .Select(material => ToGpuMaterial(material.Value, textureArrayIndices))
            .ToList();
    }

    public Material GetMaterialByName(string materialName)
    {
        return _materials.TryGetValue(materialName, out var material)
            ? material
            : _materials["M_Default_White"];
    }

    private GpuMaterial ToGpuMaterial(Material material, IDictionary<string, TextureId> textureArrayIndices)
    {
        //TODO(deccer) get rid of that guid.newguid bs
        var baseColorTextureIdExists =
            textureArrayIndices.TryGetValue(material.BaseColorTextureDataName ?? Guid.NewGuid().ToString(), out var baseColorTextureId);

        var normalTextureIdExists =
            textureArrayIndices.TryGetValue(material.NormalTextureDataName ?? Guid.NewGuid().ToString(), out var normalTextureId);

        return new GpuMaterial
        {
            Diffuse = material.BaseColor,
            Emissive = material.Emissive,
            BaseColorTextureId = baseColorTextureIdExists
                ? new Vector4i(baseColorTextureId.ArrayIndex, baseColorTextureId.ArraySlice, -1, -1)
                : new Vector4i(-1, -1, -1, -1),
            NormalTextureId = normalTextureIdExists
                ? new Vector4i(normalTextureId.ArrayIndex, normalTextureId.ArraySlice, -1, -1)
                : new Vector4i(-1, -1, -1, -1)
        };
    }
}