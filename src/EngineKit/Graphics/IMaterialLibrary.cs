using System.Collections.Generic;

namespace EngineKit.Graphics;

public interface IMaterialLibrary
{
    void AddMaterial(string name, Material material);

    IList<GpuMaterial> GetMaterialBufferData(
        string[] visibleMaterialNames,
        IDictionary<string, TextureId> textureArrayIndices,
        out IDictionary<string, int> materialNameIndexMap);

    Material GetMaterialByName(string materialName);

    bool Exists(string materialName);
}