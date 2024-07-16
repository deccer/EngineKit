using System.Collections.Generic;

namespace EngineKit.Graphics.Assets;

public interface IMeshLoader
{
    IReadOnlyCollection<MeshPrimitive> LoadMeshPrimitivesFromFile(string filePath);
}
