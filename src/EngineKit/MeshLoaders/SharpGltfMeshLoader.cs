using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using EngineKit.Graphics;
using EngineKit.Mathematics;
using Serilog;
using SharpGLTF.Schema2;
using SharpGLTF.Validation;
using Material = EngineKit.Graphics.Material;
using MeshPrimitive = EngineKit.Graphics.MeshPrimitive;

namespace EngineKit.MeshLoaders;

internal sealed class SharpGltfMeshLoader : IMeshLoader
{
    private const string SkipBecausePrimitiveHasNoVertices = "{Category}: Primitive has no vertices. Skipping";
    private const string SkipBecausePrimitiveIsNotTriangulated = "{Category}: Primitive must be triangulated. Skipping";

    private static class VertexAccessorName
    {
        public const string Position = "POSITION";
        public const string Normal = "NORMAL";
        public const string Uv0 = "TEXCOORD_0";
        public const string Uv1 = "TEXCOORD_1";
        public const string Tangent = "TANGENT";
    }

    private readonly ILogger _logger;
    private readonly IMaterialLibrary _materialLibrary;

    public SharpGltfMeshLoader(
        ILogger logger,
        IMaterialLibrary materialLibrary)
    {
        _logger = logger;
        _materialLibrary = materialLibrary;
    }

    public IReadOnlyCollection<MeshPrimitive> LoadMeshPrimitivesFromFile(string filePath)
    {
        var readSettings = new ReadSettings
        {
            Validation = ValidationMode.Skip,
        };

        var model = ModelRoot.Load(filePath, readSettings);

        Material[]? materials = null;
        if (model.LogicalMaterials.Count > 0)
        {
            materials = new Material[model.LogicalMaterials.Count];
            //Parallel.For(0, model.LogicalMaterials.Count, index =>
            for (var index = 0; index < model.LogicalMaterials.Count; index++)
            {
                var gltfMaterial = model.LogicalMaterials[index];
                if (gltfMaterial == null)
                {
                    //return;
                    continue;
                }

                var material = ProcessMaterial(gltfMaterial);
                if (material != null)
                {
                    materials[index] = material;
                }
            };
        }
        /*
        foreach (var gltfMaterial in model.LogicalMaterials)
        {
            if (gltfMaterial == null)
            {
                continue;
            }

            var material = ProcessMaterial(gltfMaterial);
            if (material != null)
            {
                materials.Add(material);
            }
        }
        */

        var meshPrimitives = new List<MeshPrimitive>(model.LogicalNodes.Count);

        var nodeStack = new Stack<ValueTuple<Node, Matrix4x4>>();
        foreach (var rootNode in model.DefaultScene.VisualChildren)
        {
            nodeStack.Push((rootNode, Matrix4x4.Identity));
        }

        while (nodeStack.Count != 0)
        {
            var (node, globalParentTransform) = nodeStack.Pop();
            var localModelMatrix = node.WorldMatrix;
            var globalModelMatrix = localModelMatrix * globalParentTransform;

            if (node.VisualChildren.Any())
            {
                foreach (var childNode in node.VisualChildren)
                {
                    nodeStack.Push((childNode, globalModelMatrix));
                }
            }

            if (node.Mesh != null)
            {
                ProcessNode(meshPrimitives, node, materials ?? []);
            }
        }

        _logger.Debug("{Category}: Loaded {PrimitiveCount} primitives from {FilePath}", nameof(SharpGltfMeshLoader),
            meshPrimitives.Count, filePath);

        return meshPrimitives;
    }

    private static ImageInformation GetImageInformationFromChannel(MaterialChannel materialChannel)
    {
        var name =
            Path.GetFileNameWithoutExtension(materialChannel.Texture?.PrimaryImage?.Content.SourcePath) ??
            $"{materialChannel.Key}-{Guid.NewGuid()}";
        return new ImageInformation(
            name,
            materialChannel.Texture?.PrimaryImage?.Content.MimeType,
            materialChannel.Texture?.PrimaryImage?.Content.Content,
            materialChannel.Texture?.PrimaryImage?.Content.SourcePath);
    }

    private Material? ProcessMaterial(SharpGLTF.Schema2.Material gltfMaterial)
    {
        var materialName = gltfMaterial.Name ?? $"Material-{Guid.NewGuid()}";

        if (_materialLibrary.Exists(materialName))
        {
            _logger.Debug("{Category}: Material {MaterialName} imported already", nameof(SharpGltfMeshLoader), materialName);
            return null;
        }

        _logger.Debug("{Category}: Loading Material {MaterialName}", nameof(SharpGltfMeshLoader), materialName);

        var material = new Material(materialName);
        foreach (var materialChannel in gltfMaterial.Channels)
        {
            if (materialChannel.Key is "BaseColor" or "Diffuse" or "RGB")
            {
                material.BaseColor = new Color4(materialChannel.Color);
                if (materialChannel.Texture != null)
                {
                    material.BaseColorImage = GetImageInformationFromChannel(materialChannel);
                }
                if (materialChannel.TextureSampler != null)
                {
                    material.BaseColorTextureSamplerInformation = new SamplerInformation(materialChannel.TextureSampler);
                }
            }
            else if (materialChannel.Key == "Normal")
            {
                // NormalScale
                if (materialChannel.Texture != null)
                {
                    material.NormalImage = GetImageInformationFromChannel(materialChannel);
                }
                if (materialChannel.TextureSampler != null)
                {
                    material.NormalTextureSamplerInformation = new SamplerInformation(materialChannel.TextureSampler);
                }
            }
            else if (materialChannel.Key == "MetallicRoughness")
            {
                material.MetallicFactor = (float?)materialChannel.Parameters.FirstOrDefault(x => x.Name == "MetallicFactor")?.Value ?? 1.0f;
                material.RoughnessFactor = (float?)materialChannel.Parameters.FirstOrDefault(x => x.Name == "RoughnessFactor")?.Value ?? 1.0f;
                if (materialChannel.Texture != null)
                {
                    material.MetalnessRoughnessImage = GetImageInformationFromChannel(materialChannel);
                }

                if (materialChannel.TextureSampler != null)
                {
                    material.MetalnessRoughnessTextureSamplerInformation =
                        new SamplerInformation(materialChannel.TextureSampler);
                }
            }
            else if (materialChannel.Key == "SpecularColor")
            {
                material.SpecularColor = new Color4(materialChannel.Color);
            }
            else if (materialChannel.Key == "SpecularFactor")
            {
                var specularFactor = (float?)materialChannel.Parameters.FirstOrDefault(x => x.Name == "SpecularFactor")?.Value ?? 1.0f;
                material.SpecularFactor = new Vector3(specularFactor);
            }
            else if (materialChannel.Key == "SpecularGlossiness")
            {
                material.SpecularFactor = (Vector3?)materialChannel.Parameters.FirstOrDefault(x => x.Name == "SpecularFactor")?.Value ?? Vector3.One;
                material.GlossinessFactor = (float?)materialChannel.Parameters.FirstOrDefault(x => x.Name == "GlossinessFactor")?.Value ?? 1.0f;

                if (materialChannel.TextureSampler != null)
                {
                    material.SpecularTextureSamplerInformation = new SamplerInformation(materialChannel.TextureSampler);
                }
            }
            else if (materialChannel.Key == "Occlusion")
            {
                material.OcclusionStrength = (float?)materialChannel.Parameters.FirstOrDefault(x => x.Name == "OcclusionStrength")?.Value ?? 1.0f;

                if (materialChannel.Texture != null)
                {
                    material.OcclusionImage = GetImageInformationFromChannel(materialChannel);
                }

                if (materialChannel.TextureSampler != null)
                {
                    material.OcclusionTextureSamplerInformation = new SamplerInformation(materialChannel.TextureSampler);
                }
            }
            else if (materialChannel.Key == "Emissive")
            {
                // Color
                material.EmissiveColor = new Color4(materialChannel.Color);
                if (materialChannel.Texture != null)
                {
                    material.EmissiveImage = GetImageInformationFromChannel(materialChannel);
                }

                if (materialChannel.TextureSampler != null)
                {
                    material.EmissiveTextureSamplerInformation = new SamplerInformation(materialChannel.TextureSampler);
                }
            }
        }

        _materialLibrary.AddMaterial(material);
        return material;
    }

    private void ProcessNode(List<MeshPrimitive> meshPrimitives, Node node, ICollection<Material> materials)
    {
        foreach (var primitive in node.Mesh.Primitives)
        {
            if (primitive?.VertexAccessors?.Keys == null)
            {
                _logger.Error(SkipBecausePrimitiveHasNoVertices, nameof(SharpGltfMeshLoader));
                continue;
            }

            if (primitive.DrawPrimitiveType != PrimitiveType.TRIANGLES)
            {
                _logger.Error(SkipBecausePrimitiveIsNotTriangulated, nameof(SharpGltfMeshLoader));
                continue;
            }

            var meshName = string.IsNullOrEmpty(node.Name)
                ? Guid.NewGuid().ToString()
                : meshPrimitives.Any(md => md.MeshName == node.Name)
                    ? node.Name + "_" + Guid.NewGuid()
                    : node.Name;

            var positions = primitive.VertexAccessors.GetValueOrDefault(VertexAccessorName.Position).AsSpan<Vector3>();
            if (positions.Length == 0)
            {
                _logger.Error("{Category}: Primitive {MeshName} has no valid vertex data", nameof(SharpGltfMeshLoader), meshName);
                continue;
            }

            var meshPrimitive = new MeshPrimitive(meshName);
            meshPrimitive.Transform = node.WorldMatrix;// .LocalTransform.Matrix;
            //meshPrimitive.MaterialName = primitive.Material?.Name ?? (primitive.Material == null ? Material.MaterialNotFoundName : materials.ElementAt(primitive.Material.LogicalIndex).Name) ?? Material.MaterialNotFoundName;

            var boundingBox = BoundingBox.CreateFromPoints(positions.ToArray());
            //boundingBox.Min = Vector3.Transform(boundingBox.Min, meshPrimitive.Transform);
            //boundingBox.Max = Vector3.Transform(boundingBox.Max, meshPrimitive.Transform);
            meshPrimitive.BoundingBox = boundingBox;

            var vertexType = GetVertexTypeFromVertexAccessorNames(primitive!.VertexAccessors!.Keys.ToList());
            var normalsAccessor = primitive.VertexAccessors.GetValueOrDefault(VertexAccessorName.Normal);
            var normals = normalsAccessor.AsSpan<Vector3>();
            var uvsAccessor = primitive.VertexAccessors.GetValueOrDefault(VertexAccessorName.Uv0);
            var uvs = uvsAccessor.AsSpan<Vector2>();
            var tangentsAccessor = primitive.VertexAccessors.GetValueOrDefault(VertexAccessorName.Tangent);
            var realTangents = tangentsAccessor.AsSpan<Vector4>();
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
                meshPrimitive.AddIndices(indices);
            }
            else
            {
                Debugger.Break();
            }

            var emptyNormal = Vector3.Zero;

            for (var i = 0; i < positions.Length; i++)
            {
                ref var position = ref positions[i];
                ref var normal = ref normals[normals.Length == 1 ? 0 : i];

                var realTangentXyz = new Vector3(realTangents[i].X, realTangents[i].Y, realTangents[i].Z);
                var realTangent = new Vector4(realTangentXyz, realTangents[i].W);

                switch (vertexType)
                {
                    case VertexType.PositionNormal:
                        meshPrimitive.AddPositionNormal(position, normal);
                        break;
                    case VertexType.PositionNormalUv:
                        meshPrimitive.AddPositionNormalUvRealTangent(position, normal, uvs[i], Vector4.One);
                        break;
                    case VertexType.PositionNormalUvTangent:
                        meshPrimitive.AddPositionNormalUvRealTangent(
                            position,
                            normal,
                            uvs[i],
                            realTangent);
                        break;
                    case VertexType.PositionUv:
                        meshPrimitive.AddPositionUv(position, uvs[i]);
                        break;
                    default:
                    {
                        meshPrimitive.AddPositionUv(position, uvs[i]);
                        break;
                    }
                }
            }
            meshPrimitives.Add(meshPrimitive);
        }
    }

    private static VertexType GetVertexTypeFromVertexAccessorNames(ICollection<string> vertexAccessorNames)
    {
        if (vertexAccessorNames.Contains(VertexAccessorName.Position))
        {
            if (vertexAccessorNames.Contains(VertexAccessorName.Normal))
            {
                if (vertexAccessorNames.Contains(VertexAccessorName.Uv0))
                {
                    if (vertexAccessorNames.Contains(VertexAccessorName.Tangent))
                    {
                        return VertexType.PositionNormalUvTangent;
                    }

                    return VertexType.PositionNormalUv;
                }

                return VertexType.PositionNormal;
            }

            if (vertexAccessorNames.Contains(VertexAccessorName.Uv0))
            {
                return VertexType.PositionUv;
            }
        }

        return VertexType.Unknown;
    }
}
