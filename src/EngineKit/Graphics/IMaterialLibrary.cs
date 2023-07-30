using System.Collections.Generic;

namespace EngineKit.Graphics;

public interface IMaterialLibrary
{
    void AddMaterial(Material material);

    void RemoveMaterial(string name);

    IList<string> GetMaterialNames();

    Material GetMaterialByName(string? materialName);

    Material GetRandomMaterial();

    bool Exists(string materialName);
}