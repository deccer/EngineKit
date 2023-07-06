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
using Num = System.Numerics;
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

    public IReadOnlyCollection<MeshPrimitive> LoadMeshPrimitivesFromFile(string filePath)
    {
        //var runtimeOptions = new RuntimeOptions().GpuMeshInstancing == MeshInstancing.SingleMesh;
        var readSettings = new ReadSettings
        {
            Validation = ValidationMode.Skip
        };

        var model = ModelRoot.Load(filePath, readSettings);

        var materials = new List<Material>(128);
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

        var meshPrimitives = new List<MeshPrimitive>(model.LogicalNodes.Count);
        foreach (var node in model.LogicalNodes)
        {
            if (node.Mesh == null)
            {
                continue;
            }

            ProcessNode(meshPrimitives, node, materials);
        }

        _logger.Debug("{Category}: Loaded {PrimitiveCount} primitives from {FilePath}", nameof(SharpGltfMeshLoader),
            meshPrimitives.Count, filePath);

        return meshPrimitives;
    }

    private static ImageInformation GetImageInformationFromChannel(MaterialChannel materialChannel)
    {
        var name =
            Path.GetFileNameWithoutExtension(materialChannel.Texture?.PrimaryImage?.Content.SourcePath) ??
            Guid.NewGuid().ToString();
        return new ImageInformation(
            name,
            materialChannel.Texture?.PrimaryImage?.Content.MimeType,
            materialChannel.Texture?.PrimaryImage?.Content.Content,
            materialChannel.Texture?.PrimaryImage?.Content.SourcePath);
    }

    private Material? ProcessMaterial(SharpGLTF.Schema2.Material gltfMaterial)
    {
        var materialName = gltfMaterial.Name ?? Guid.NewGuid().ToString();

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
                material.BaseColor = new Color4(
                    materialChannel.Color.X,
                    materialChannel.Color.Y,
                    materialChannel.Color.Z,
                    materialChannel.Color.W);
                
                if (materialChannel.Texture?.PrimaryImage != null)
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
                if (materialChannel.Texture?.PrimaryImage != null)
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

                if (materialChannel.Texture?.PrimaryImage != null)
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
                material.SpecularColor = new Color4(materialChannel.Color.X, materialChannel.Color.Y, materialChannel.Color.Z, materialChannel.Color.W);
            }
            else if (materialChannel.Key == "SpecularFactor")
            {
                var specularFactor = (float?)materialChannel.Parameters.FirstOrDefault(x => x.Name == "SpecularFactor")?.Value ?? 1.0f;
                material.SpecularFactor = new Num.Vector3(specularFactor);
            }
            else if (materialChannel.Key == "SpecularGlossiness")
            {
                material.SpecularFactor = (Num.Vector3?)materialChannel.Parameters.FirstOrDefault(x => x.Name == "SpecularFactor")?.Value ?? Num.Vector3.One;
                material.GlossinessFactor = (float?)materialChannel.Parameters.FirstOrDefault(x => x.Name == "GlossinessFactor")?.Value ?? 1.0f;

                if (materialChannel.TextureSampler != null)
                {
                    material.SpecularTextureSamplerInformation = new SamplerInformation(materialChannel.TextureSampler);
                }
            }
            else if (materialChannel.Key == "Occlusion")
            {
                material.OcclusionStrength = (float?)materialChannel.Parameters.FirstOrDefault(x => x.Name == "OcclusionStrength")?.Value ?? 1.0f;
                
                if (materialChannel.Texture?.PrimaryImage != null)
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
                material.EmissiveColor = new Color4(materialChannel.Color.X, materialChannel.Color.Y, materialChannel.Color.Z, 0.0f);
                
                if (materialChannel.Texture?.PrimaryImage != null)
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

    private void ProcessNode(ICollection<MeshPrimitive> meshPrimitives, Node node, ICollection<Material> materials)
    {
        foreach (var primitive in node.Mesh.Primitives)
        {
            if (primitive?.VertexAccessors?.Keys == null)
            {
                _logger.Error("{Category}: Primitives has no vertices. Skipping", nameof(SharpGltfMeshLoader));
                continue;
            }

            if (primitive.DrawPrimitiveType != PrimitiveType.TRIANGLES)
            {
                _logger.Error("{Category}: Only triangle primitives are allowed", nameof(SharpGltfMeshLoader));
                continue;
            }

            var meshName = string.IsNullOrEmpty(node.Name)
                ? Guid.NewGuid().ToString()
                : meshPrimitives.Any(md => md.MeshName == node.Name)
                    ? node.Name + "_" + Guid.NewGuid()
                    : node.Name;
            
            var positions = primitive.VertexAccessors.GetValueOrDefault("POSITION").AsSpan<Vector3>();
            if (positions.Length == 0)
            {
                _logger.Error("{Category}: Mesh primitive {MeshName} has no valid vertex data", nameof(SharpGltfMeshLoader), meshName);
                continue;
            }      
            
            var meshPrimitive = new MeshPrimitive(meshName);
            meshPrimitive.Transform = node.WorldMatrix.ToMatrix();
            meshPrimitive.MaterialName = primitive.Material?.Name ?? (primitive.Material == null ? "M_NotFound" : materials.ElementAt(primitive.Material.LogicalIndex)?.Name) ?? "M_NotFound";
            
            var vertexType = GetVertexTypeFromVertexAccessorNames(primitive!.VertexAccessors!.Keys.ToList());

            meshPrimitive.BoundingBox = BoundingBox.FromPoints(positions.ToArray());
            
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
                meshPrimitive.AddIndices(indices);
            }
            else
            {
                Debugger.Break();
            }

            for (var i = 0; i < positions.Length; i++)
            {
                var position = Vector3.TransformPosition(positions[i], meshPrimitive.Transform);
                var normal = Vector3.TransformDirection(normals[i], meshPrimitive.Transform);
                var realTangentXyz = new Vector3(realTangents[i].X, realTangents[i].Y, realTangents[i].Z);
                var realTangent = new Vector4(Vector3.TransformDirection(realTangentXyz, meshPrimitive.Transform), realTangents[i].W);

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
                        if (vertexType == VertexType.PositionUv)
                        {
                            meshPrimitive.AddPositionUv(position, uvs[i]);
                        }

                        break;
                    }
                }
            }
            meshPrimitives.Add(meshPrimitive);
        }
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