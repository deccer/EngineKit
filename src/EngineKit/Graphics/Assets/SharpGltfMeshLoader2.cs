namespace EngineKit.Graphics.Assets;

using SharpGLTF.Schema2;
using SharpGLTF.Validation;
using System.Collections.Generic;
using MeshPrimitive = EngineKit.Graphics.MeshPrimitive;

internal sealed class SharpGltfMeshLoader2 : IMeshLoader
{
    public IReadOnlyCollection<MeshPrimitive> LoadMeshPrimitivesFromFile(string filePath)
    {
        var readSettings = new ReadSettings
                           {
                               Validation = ValidationMode.Skip,
                           };

        var model = ModelRoot.Load(filePath, readSettings);


        return new List<MeshPrimitive>();
    }
}
