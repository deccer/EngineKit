using System;
using EngineKit.Mathematics;

namespace EngineKit.Graphics;

public record Material
{
    public string Name { get; set; }

    public Color4 BaseColor { get; set; }

    public Color4 Emissive { get; set; }

    public string? BaseColorTextureFilePath { get; set; }

    public ReadOnlyMemory<byte>? BaseColorEmbeddedImageData { get; set; }

    public string? BaseColorTextureDataName { get; set; }

    public string? NormalTextureFilePath { get; set; }

    public ReadOnlyMemory<byte>? NormalEmbeddedImageData { get; set; }

    public string? NormalTextureDataName { get; set; }

    public string? SpecularTextureFilePath { get; set; }

    public ReadOnlyMemory<byte>? SpecularEmbeddedImageData { get; set; }
}