using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace EngineKit.Graphics;

internal sealed class MeshFactory : IMeshFactory
{
    private readonly ILogger _logger;

    public MeshFactory(ILogger logger)
    {
        _logger = logger.ForContext<MeshFactory>();
    }

    public IVertexBuffer CreateVertexBuffer(MeshData[] meshDates, VertexType targetVertexType)
    {
        var bufferData = new List<VertexPositionNormalUvTangent>(1_024_000);
        foreach (var meshData in meshDates)
        {
            if (!meshData.RealTangents.Any())
            {
                meshData.CalculateTangents();
            }

            for (var i = 0; i < meshData.Positions.Count; ++i)
            {
                bufferData.Add(new VertexPositionNormalUvTangent(
                    meshData.Positions[i],
                    meshData.Normals[i],
                    meshData.Uvs[i],
                    meshData.RealTangents[i]));
            }
        }

        return new VertexBuffer<VertexPositionNormalUvTangent>("Vertices", bufferData.ToArray());
    }

    public IIndexBuffer CreateIndexBuffer(MeshData[] meshDates)
    {
        var indices = meshDates
            .SelectMany(meshData => meshData.Indices)
            .ToArray();
        return new IndexBuffer<uint>("Indices", indices);
    }
}