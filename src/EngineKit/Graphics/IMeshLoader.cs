using System.Collections.Generic;

namespace EngineKit.Graphics;

public interface IMeshLoader
{
    IReadOnlyCollection<MeshData> LoadModel(string filePath);
}