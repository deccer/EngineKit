using System.Collections.Generic;

namespace EngineKit.Graphics;
public interface IMeshLoader
{
    IReadOnlyCollection<MeshPrimitive> LoadMeshPrimitivesFromFile(string filePath);
}