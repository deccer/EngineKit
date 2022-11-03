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
using Vector2 = EngineKit.Mathematics.Vector2;
using Vector3 = EngineKit.Mathematics.Vector3;
using Vector4 = EngineKit.Mathematics.Vector4;

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

        foreach (var material in model.LogicalMaterials)
        {
            if (material != null)
            {
                ProcessMaterial(material);
            }
        }

        var meshDates = new List<MeshData>(model.LogicalMeshes.Count);
        foreach (var node in model.LogicalNodes)
        {
            if (node.Mesh == null)
            {
                continue;
            }

            ProcessNode(meshDates, node);
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

        var material = new Material(gltfMaterial.Name ?? Guid.NewGuid().ToString());

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

        foreach (var primitive in node.Mesh.Primitives)
        {
            meshData.MaterialName = primitive?.Material?.Name ?? "Unnamed Material";
            if (primitive!.DrawPrimitiveType != PrimitiveType.TRIANGLES)
            {
                _logger.Error("{Category}: Only triangle primitives are allowed", nameof(SharpGltfMeshLoader));
                continue;
            }

            var vertexType = GetVertexTypeFromVertexAccessorNames(primitive.VertexAccessors.Keys.ToList());

            var positions = primitive.VertexAccessors.GetValueOrDefault("POSITION").AsSpan<Vector3>();
            var normals = primitive.VertexAccessors.GetValueOrDefault("NORMAL").AsSpan<Vector3>();
            var uvs = primitive.VertexAccessors.GetValueOrDefault("TEXCOORD_0").AsSpan<Vector2>();
            var realTangents = primitive.VertexAccessors.GetValueOrDefault("TANGENT").AsSpan<Vector4>();
            if (uvs.Length == 0)
            {
                uvs = new Vector2[positions.Length].AsSpan();
            }

            if (realTangents.Length == 0)
            {
                realTangents = new Vector4[positions.Length].AsSpan();
            }

            if (primitive.IndexAccessor != null)
            {
                var indices = primitive.IndexAccessor.AsIndicesArray().ToArray();
                meshData.AddIndices(indices);
            }
            else
            {
                Debugger.Break();
            }

            for (var i = 0; i < positions.Length; i++)
            {
                var position = Vector3.TransformPosition(positions[i], meshData.Transform);
                var normal = Vector3.TransformDirection(normals[i], meshData.Transform);
                var realTangentXyz = new Vector3(realTangents[i].X, realTangents[i].Y, realTangents[i].Z);
                var realTangent = new Vector4(Vector3.TransformDirection(realTangentXyz, meshData.Transform), realTangents[i].W);

                switch (vertexType)
                {
                    case VertexType.PositionNormal:
                        meshData.AddPositionNormal(position, normal);
                        break;
                    case VertexType.PositionNormalUv:
                        meshData.AddPositionNormalUvRealTangent(position, normal, uvs[i], Vector4.One);
                        break;
                    case VertexType.PositionNormalUvTangent:
                        meshData.AddPositionNormalUvRealTangent(
                            position,
                            normal,
                            uvs[i],
                            realTangent);
                        break;
                    case VertexType.PositionUv:
                        meshData.AddPositionUv(position, uvs[i]);
                        break;
                    default:
                    {
                        if (vertexType == VertexType.PositionUv)
                        {
                            meshData.AddPositionUv(position, uvs[i]);
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