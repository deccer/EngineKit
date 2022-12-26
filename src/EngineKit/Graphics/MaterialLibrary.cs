using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using Serilog;

namespace EngineKit.Graphics;

internal sealed class MaterialLibrary : IMaterialLibrary
{
    private readonly ILogger _logger;
    private readonly IImageLibrary _imageLibrary;
    private readonly IDictionary<string, Material> _materials;
    private readonly Random _random;

    public MaterialLibrary(
        ILogger logger,
        IImageLibrary imageLibrary)
    {
        _logger = logger.ForContext<MaterialLibrary>();
        _imageLibrary = imageLibrary;
        _materials = new Dictionary<string, Material>(256);
        _random = new Random();
        CreateDefaultMaterials();
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

        if (!string.IsNullOrEmpty(material.SpecularTextureDataName))
        {
            if (material.SpecularEmbeddedImageData.HasValue)
            {
                var imageSpan = material.SpecularEmbeddedImageData.Value.Span;
                _imageLibrary.AddImage(material.SpecularTextureDataName, imageSpan);
            }
            else
            {
                if (!string.IsNullOrEmpty(material.SpecularTextureFilePath))
                {
                    _imageLibrary.AddImage(material.SpecularTextureDataName, material.SpecularTextureFilePath);
                }
            }
        }

        if (!string.IsNullOrEmpty(material.MetalnessRoughnessTextureDataName))
        {
            if (material.MetalnessRoughnessEmbeddedImageData.HasValue)
            {
                var imageSpan = material.MetalnessRoughnessEmbeddedImageData.Value.Span;
                _imageLibrary.AddImage(material.MetalnessRoughnessTextureDataName, imageSpan);
            }
            else
            {
                if (!string.IsNullOrEmpty(material.MetalnessRoughnessTextureFilePath))
                {
                    _imageLibrary.AddImage(material.MetalnessRoughnessTextureDataName, material.MetalnessRoughnessTextureFilePath);
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

    public IList<string> GetMaterialNames()
    {
        return _materials.Keys.ToList();
    }

    public Material GetMaterialByName(string materialName)
    {
        return _materials.TryGetValue(materialName, out var material)
            ? material
            : _materials["M_Default_White"];
    }

    private static GpuMaterial ToGpuMaterial(Material material, IDictionary<string, TextureId> textureArrayIndices)
    {
        //TODO(deccer) get rid of that guid.newguid bs
        var baseColorTextureIdExists =
            textureArrayIndices.TryGetValue(material.BaseColorTextureDataName ?? Guid.NewGuid().ToString(), out var baseColorTextureId);

        var normalTextureIdExists =
            textureArrayIndices.TryGetValue(material.NormalTextureDataName ?? Guid.NewGuid().ToString(), out var normalTextureId);

        var specularTextureIdExists =
            textureArrayIndices.TryGetValue(material.SpecularTextureDataName ?? Guid.NewGuid().ToString(), out var specularTextureId);

        var metalnessRoughnessTextureIdExists =
            textureArrayIndices.TryGetValue(material.MetalnessRoughnessTextureDataName ?? Guid.NewGuid().ToString(), out var metalnessRoughnessTextureId);

        return new GpuMaterial
        {
            BaseColor = material.BaseColor,
            Emissive = material.Emissive,
            BaseColorTextureId = baseColorTextureIdExists
                ? new Vector4i(baseColorTextureId!.ArrayIndex, baseColorTextureId.ArraySlice, -1, -1)
                : new Vector4i(-1, -1, -1, -1),
            NormalTextureId = normalTextureIdExists
                ? new Vector4i(normalTextureId!.ArrayIndex, normalTextureId.ArraySlice, -1, -1)
                : new Vector4i(-1, -1, -1, -1),
            SpecularTextureId = specularTextureIdExists
                ? new Vector4i(specularTextureId!.ArrayIndex, specularTextureId.ArraySlice, -1, -1)
                : new Vector4i(-1, -1, -1, -1),
            MetalnessRoughnessTextureId = metalnessRoughnessTextureIdExists
                ? new Vector4i(metalnessRoughnessTextureId!.ArrayIndex, metalnessRoughnessTextureId.ArraySlice, -1, -1)
                : new Vector4i(-1, -1, -1, -1),
        };
    }

    private void CreateDefaultMaterials()
    {
        var colors = typeof(Color4)
            .GetFields()
            .Where(field => field.IsStatic && field.IsPublic);

        foreach (var color in colors)
        {
            var materialName = $"M_Default_{color.Name}";
            var colorValue = (Color4?)color.GetValue(color);
            _materials.Add(materialName, new Material(materialName)
            {
                BaseColor = colorValue ?? Color4.Fuchsia
            });
        }
    }
}