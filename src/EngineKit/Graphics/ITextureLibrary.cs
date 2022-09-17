using System.Collections.Generic;

namespace EngineKit.Graphics;

public interface ITextureLibrary
{
    IReadOnlyCollection<ITexture> PrepareTextureArrays(
        IDictionary<string, IList<ImageLibraryItem>> imageDataPerMaterial,
        out IDictionary<string, TextureId> textureArrayIndices);
}