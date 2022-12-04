using System;
using OpenTK.Mathematics;

namespace EngineKit.Graphics;

public record Material(string Name)
{
    public string Name { get; set; } = Name;

    public float MetallicFactor { get; set; }

    public float RoughnessFactor { get; set; }

    public Color4 BaseColor { get; set; }

    public Color4 Emissive { get; set; }

    public string? BaseColorTextureDataName { get; set; }

    public string? BaseColorTextureFilePath { get; set; }

    public ReadOnlyMemory<byte>? BaseColorEmbeddedImageData { get; set; }

    public string? NormalTextureDataName { get; set; }

    public string? NormalTextureFilePath { get; set; }

    public ReadOnlyMemory<byte>? NormalEmbeddedImageData { get; set; }

    public string? SpecularTextureDataName { get; set; }

    public string? SpecularTextureFilePath { get; set; }

    public ReadOnlyMemory<byte>? SpecularEmbeddedImageData { get; set; }

    public string? MetalnessRoughnessTextureDataName { get; set; }

    public string? MetalnessRoughnessTextureFilePath { get; set; }

    public ReadOnlyMemory<byte>? MetalnessRoughnessEmbeddedImageData { get; set; }
}