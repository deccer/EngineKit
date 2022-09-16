using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace EngineKit.Graphics;

public interface IImageLibrary
{
    void AddImage(string name, string filePath);

    void AddImage(string name, ReadOnlySpan<byte> imageSpan);

    IDictionary<string, IList<ImageLibraryItem>> GetImageDataPerMaterial(IImmutableList<Material> materials);
    bool FlipHorizontal { get; set; }
    bool FlipVertical { get; set; }
}