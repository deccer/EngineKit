using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using EngineKit.Extensions;
using EngineKit.Mathematics;
using Serilog;
using SharpGLTF.Schema2;
using SharpGLTF.Validation;

namespace EngineKit.Graphics.MeshLoaders;

internal sealed class SharpGltfMeshLoader : IMeshLoader
{
    private readonly ILogger _logger;
    private readonly IMaterialLibrary _materialLibrary;

    public SharpGltfMeshLoader(
        ILogger logger,
        IMaterialLibrary materialLibrary)
    {
        _logger = logger;
        _materialLibrary = materialLibrary;
    }

    public IReadOnlyCollection<MeshData> LoadModel(string filePath)
    {
        //var runtimeOptions = new RuntimeOptions().GpuMeshInstancing == MeshInstancing.SingleMesh;
        var readSettings = new ReadSettings
        {
            Validation = ValidationMode.Skip
        };

        var model = ModelRoot.Load(filePath, readSettings);

        foreach (var texture in model.LogicalTextures)
        {
            //ProcessImage(texture)
        }

        foreach (var material in model.LogicalMaterials)
        {
            if (material != null)
            {
                ProcessMaterial(material);
            }
        }

        var positions = new List<Vector3>(16384);
        var normals = new List<Vector3>(16384);
        var uvs = new List<Vector2>(16384);
        var realTangents = new List<Vector4>(16384);

        var meshDates = new List<MeshData>(model.LogicalMeshes.Count);
        foreach (var mesh in model.LogicalMeshes)
        {
            var node = model.LogicalNodes.FirstOrDefault(o => o.Mesh == mesh);
            foreach (var primitive in mesh.Primitives)
            {
                positions.Clear();
                normals.Clear();
                uvs.Clear();
                realTangents.Clear();
                var meshData = new MeshData(mesh.Name + primitive.LogicalIndex);
                meshData.Transform = node.WorldMatrix.ToMatrix();
                meshData.MaterialName = primitive.Material?.Name ?? "Unnamed Material";

                var vertexType = GetVertexTypeFromVertexAccessorNames(primitive.VertexAccessors.Keys.ToList());

                positions.Clear();
                if (primitive.VertexAccessors.TryGetValue("POSITION", out var positionAccessor))
                {
                    positions = positionAccessor
                        .AsVector3Array()
                        .Select(position =>
                            Vector3.TransformPosition(new Vector3(position.X, position.Y, position.Z), meshData.Transform))
                        .ToList();
                }

                if (positions.Count == 0)
                {
                    continue;
                }

                normals.Clear();
                if (primitive.VertexAccessors.TryGetValue("NORMAL", out var normalAccessor))
                {
                    normals = normalAccessor
                        .AsVector3Array()
                        .Select(normal => new Vector3(normal.X, normal.Y, normal.Z))
                        .ToList();
                }

                uvs.Clear();
                if (primitive.VertexAccessors.TryGetValue("TEXCOORD_0", out var uvAccessor))
                {
                    uvs = uvAccessor
                        .AsVector2Array()
                        .Select(uv => new Vector2(uv.X, uv.Y))
                        .ToList();
                }

                realTangents.Clear();
                if (primitive.VertexAccessors.TryGetValue("TANGENT", out var tangentAccessor))
                {
                    realTangents = tangentAccessor
                        .AsVector4Array()
                        .Select(tangent => new Vector4(tangent.X, tangent.Y, tangent.Z, tangent.W))
                        .ToList();
                }

                var indexCount = 0;
                if (primitive.IndexAccessor != null)
                {
                    var indices = primitive.IndexAccessor.AsIndicesArray().ToArray();
                    indexCount = indices.Length;
                    meshData.AddIndices(indices);
                }
                else
                {
                    Debugger.Break();
                }


                for (var i = 0; i < positions.Count; i++)
                {
                    switch (vertexType)
                    {
                        case VertexType.PositionNormal:
                            meshData.AddPositionNormal(positions[i], normals[i]);
                            break;
                        case VertexType.PositionNormalUv:
                            meshData.AddPositionNormalUvRealTangent(positions[i], normals[i], uvs[i], Vector4.One);
                            break;
                        case VertexType.PositionNormalUvTangent:
                            meshData.AddPositionNormalUvRealTangent(
                                positions[i],
                                normals[i],
                                uvs[i],
                                realTangents[i]);
                            break;
                        case VertexType.PositionUv:
                            meshData.AddPositionUv(positions[i], uvs[i]);
                            break;
                        default:
                        {
                            if (vertexType == VertexType.PositionUv)
                            {
                                meshData.AddPositionUv(positions[i], uvs[i]);
                            }

                            break;
                        }
                    }
                }

                meshDates.Add(meshData);
            }
        }

        _logger.Debug("{Category}: Loaded {PrimitiveCount} primitives from {FilePath}", nameof(SharpGltfMeshLoader),
            meshDates.Count, filePath);

        return meshDates;
    }

    private void ProcessMaterial(SharpGLTF.Schema2.Material gltfMaterial)
    {
        // BaseColor
        // MetallicRoughness
        // Normal
        // Occlusion
        // Emissive

        if (string.IsNullOrEmpty(gltfMaterial.Name))
        {
            _logger.Error("{Category}: Material has no name, skipping import", nameof(SharpGltfMeshLoader));
            return;
        }

        if (_materialLibrary.Exists(gltfMaterial.Name))
        {
            _logger.Debug("{Category}: Material {MaterialName} imported already", nameof(SharpGltfMeshLoader), gltfMaterial.Name);
            return;
        }

        var material = new Material();
        material.Name = gltfMaterial.Name ?? Guid.NewGuid().ToString();

        foreach (var materialChannel in gltfMaterial.Channels)
        {
            if (materialChannel.Key is "BaseColor" or "Diffuse")
            {
                material.BaseColor = new Color4(
                    materialChannel.Color.X,
                    materialChannel.Color.Y,
                    materialChannel.Color.Z,
                    materialChannel.Color.W);

                if (materialChannel.Texture?.PrimaryImage != null)
                {
                    material.BaseColorTextureDataName = Path.GetFileNameWithoutExtension(materialChannel.Texture?.PrimaryImage?.Name) ?? Guid.NewGuid().ToString();
                    if (materialChannel.Texture.PrimaryImage.Content.IsEmpty)
                    {
                        material.BaseColorTextureFilePath = materialChannel.Texture?.PrimaryImage?.Content.SourcePath;
                    }
                    else
                    {
                        material.BaseColorEmbeddedImageData = materialChannel.Texture.PrimaryImage.Content.Content;
                    }
                }
            }
            else if (materialChannel.Key == "Normal")
            {
                if (materialChannel.Texture?.PrimaryImage != null)
                {
                    material.NormalTextureDataName = Path.GetFileNameWithoutExtension(materialChannel.Texture?.PrimaryImage?.Name) ?? Guid.NewGuid().ToString();
                    if (materialChannel.Texture.PrimaryImage.Content.IsEmpty)
                    {
                        material.NormalTextureFilePath = materialChannel.Texture?.PrimaryImage?.Content.SourcePath;
                    }
                    else
                    {
                        material.NormalEmbeddedImageData = materialChannel.Texture.PrimaryImage.Content.Content;
                    }
                }

                // NormalScale
            }
            else if (materialChannel.Key == "MetallicRoughness")
            {
                // MetallicFactor
                // RoughnessFactor
                // metallicRoughnessTexture
            }
            else if (materialChannel.Key == "SpecularGlossiness")
            {
                // SpecularFactor (vec3) (parameters[0])
                // GlossinessFactor (float) (parameters[1])
            }
            else if (materialChannel.Key == "Occlusion")
            {
                // OcclusionStrength
            }
            else if (materialChannel.Key == "RGB")
            {
                // Value
            }
            else if (materialChannel.Key == "Emissive")
            {
                // Color
            }
        }

        _materialLibrary.AddMaterial(material.Name, material);
    }

    private void ProcessNode(ICollection<MeshData> meshDates, Node node)
    {
        if (node.Mesh == null)
        {
            _logger.Debug("{Category}: No mesh found in node {NodeName}", nameof(SharpGltfMeshLoader), node.Name);
            return;
        }

        var meshData = new MeshData(node.Name ?? Guid.NewGuid().ToString());
        meshData.Transform = node.WorldMatrix.ToMatrix();

        var positions = new List<Vector3>(16384);
        var normals = new List<Vector3>(16384);
        var uvs = new List<Vector2>(16384);
        var realTangents = new List<Vector4>(16384);

        foreach (var primitive in node.Mesh.Primitives)
        {
            meshData.MaterialName = primitive?.Material?.Name ?? "Unnamed Material";
            if (primitive!.DrawPrimitiveType != PrimitiveType.TRIANGLES)
            {
                _logger.Error("{Category}: Only triangle primitives are allowed", nameof(SharpGltfMeshLoader));
                continue;
            }

            var vertexType = GetVertexTypeFromVertexAccessorNames(primitive.VertexAccessors.Keys.ToList());

            positions.Clear();
            if (primitive.VertexAccessors.TryGetValue("POSITION", out var positionAccessor))
            {
                positions = positionAccessor
                    .AsVector3Array()
                    .Select(position =>
                        Vector3.TransformPosition(new Vector3(position.X, position.Y, position.Z), meshData.Transform))
                    .ToList();
            }

            if (positions.Count == 0)
            {
                continue;
            }

            normals.Clear();
            if (primitive.VertexAccessors.TryGetValue("NORMAL", out var normalAccessor))
            {
                normals = normalAccessor
                    .AsVector3Array()
                    .Select(normal => new Vector3(normal.X, normal.Y, normal.Z))
                    .ToList();
            }

            uvs.Clear();
            if (primitive.VertexAccessors.TryGetValue("TEXCOORD_0", out var uvAccessor))
            {
                uvs = uvAccessor
                    .AsVector2Array()
                    .Select(uv => new Vector2(uv.X, uv.Y))
                    .ToList();
            }

            realTangents.Clear();
            if (primitive.VertexAccessors.TryGetValue("TANGENT", out var tangentAccessor))
            {
                realTangents = tangentAccessor
                    .AsVector4Array()
                    .Select(tangent => new Vector4(tangent.X, tangent.Y, tangent.Z, tangent.W))
                    .ToList();
            }

            var indexCount = 0;
            if (primitive.IndexAccessor != null)
            {
                var indices = primitive.IndexAccessor.AsIndicesArray().ToArray();
                indexCount = indices.Length;
                meshData.AddIndices(indices);
            }
            else
            {
                Debugger.Break();
            }

            /*
            _logger.Debug(
                "{Category}: Processing Primitive {VertexType} - {IndexType} I:{IndexCount} P:{PositionCount} N:{NormalCount} U:{UvCount} T:{TangentCount}",
                nameof(MeshFactory),
                vertexType,
                primitive.IndexAccessor?.Encoding ?? EncodingType.FLOAT,
                indexCount,
                positions.Count,
                normals.Count,
                uvs.Count,
                realTangents.Count);
                */

            for (var i = 0; i < positions.Count; i++)
            {
                switch (vertexType)
                {
                    case VertexType.PositionNormal:
                        meshData.AddPositionNormal(positions[i], normals[i]);
                        break;
                    case VertexType.PositionNormalUv:
                        meshData.AddPositionNormalUvRealTangent(positions[i], normals[i], uvs[i], Vector4.One);
                        break;
                    case VertexType.PositionNormalUvTangent:
                        meshData.AddPositionNormalUvRealTangent(
                            positions[i],
                            normals[i],
                            uvs[i],
                            realTangents[i]);
                        break;
                    case VertexType.PositionUv:
                        meshData.AddPositionUv(positions[i], uvs[i]);
                        break;
                    default:
                    {
                        if (vertexType == VertexType.PositionUv)
                        {
                            meshData.AddPositionUv(positions[i], uvs[i]);
                        }

                        break;
                    }
                }
            }
        }

        meshDates.Add(meshData);
    }

    private static VertexType GetVertexTypeFromVertexAccessorNames(ICollection<string> vertexAccessorNames)
    {
        if (vertexAccessorNames.Contains("POSITION"))
        {
            if (vertexAccessorNames.Contains("NORMAL"))
            {
                if (vertexAccessorNames.Contains("TEXCOORD_0"))
                {
                    if (vertexAccessorNames.Contains("TANGENT"))
                    {
                        return VertexType.PositionNormalUvTangent;
                    }

                    return VertexType.PositionNormalUv;
                }

                return VertexType.PositionNormal;
            }

            if (vertexAccessorNames.Contains("TEXCOORD_0"))
            {
                return VertexType.PositionUv;
            }
        }

        return VertexType.Unknown;
    }
}