using System.Collections.Generic;
using SixLabors.ImageSharp;

namespace EngineKit.Graphics;

public interface ITextureLibrary
{
    void AddTexture(string name, string fileName);

    void AddTexture(string name, Image image);

    void LoadTextures();

    ulong GetTextureHandle(string textureName);

    IReadOnlyCollection<ITexture> PrepareTextureArrays(
        IDictionary<string, IList<ImageLibraryItem>> imageDataPerMaterial,
        out IDictionary<string, TextureId> textureArrayIndices);
}