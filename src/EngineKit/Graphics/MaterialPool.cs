using System.Collections.Generic;
using Serilog;

namespace EngineKit.Graphics;

internal sealed class MaterialPool : IMaterialPool
{
    private readonly ILogger _logger;
    private readonly ICapabilities _capabilities;
    private readonly IGraphicsContext _graphicsContext;
    private readonly ISamplerLibrary _samplerLibrary;
    private readonly IDictionary<Material, PooledMaterial> _pooledMaterials;
    private readonly IDictionary<string, ITexture> _textures;

    private readonly ISampler _sampler;

    public MaterialPool(
        ILogger logger,
        Label label,
        ICapabilities capabilities,
        IGraphicsContext graphicsContext,
        ISamplerLibrary samplerLibrary,
        int materialBufferCapacity)
    {
        _logger = logger.ForContext<MaterialPool>();
        _capabilities = capabilities;
        _graphicsContext = graphicsContext;
        _samplerLibrary = samplerLibrary;
        _pooledMaterials = new Dictionary<Material, PooledMaterial>();
        _textures = new Dictionary<string, ITexture>();

        _sampler = _graphicsContext.CreateSampler(new SamplerDescriptor
        {
            Label = "Sampler-MaterialPool",
            TextureAddressModeU = TextureAddressMode.ClampToBorder,
            TextureAddressModeV = TextureAddressMode.ClampToBorder,
            TextureAddressModeW = TextureAddressMode.ClampToBorder,
            InterpolationFilter = TextureInterpolationFilter.Linear,
            MipmapFilter = TextureMipmapFilter.LinearMipmapLinear,
            LodBias = 0.0f,
            MinLod = -1000.0f,
            MaxLod = 1000.0f
        });
        
        MaterialBuffer = graphicsContext.CreateShaderStorageBuffer<GpuMaterial>(label);
        MaterialBuffer.AllocateStorage(materialBufferCapacity, StorageAllocationFlags.Dynamic);
    }

    public IBuffer MaterialBuffer { get; }

    public void Dispose()
    {
        foreach (var texture in _textures.Values)
        {
            texture.Dispose();
        }
        _textures.Clear();
        _sampler.Dispose();
        MaterialBuffer.Dispose();
    }

    public PooledMaterial GetOrAdd(Material material)
    {
        if (_pooledMaterials.TryGetValue(material, out var pooledMaterial))
        {
            return pooledMaterial;
        }

        if (!material.TexturesLoaded)
        {
            material.LoadTextures(_logger, _graphicsContext, _samplerLibrary, _textures, _capabilities.SupportsBindlessTextures);
        }

        var gpuMaterial = new GpuMaterial
        {
            BaseColorFactor = material.BaseColor,
            BaseColorTexture = material.BaseColorTexture?.TextureHandle ?? 0,
            NormalTexture = material.NormalTexture?.TextureHandle ?? 0,
            MetallicFactor = material.MetallicFactor,
            RoughnessFactor = material.RoughnessFactor,
            MetalnessRoughnessTexture = material.MetalnessRoughnessTexture?.TextureHandle ?? 0,
            SpecularTexture = material.SpecularTexture?.TextureHandle ?? 0,
            OcclusionTexture = material.OcclusionTexture?.TextureHandle ?? 0,
            EmissiveFactor = material.EmissiveColor.ToVector4(),
            EmissiveTexture = material.EmissiveTexture?.TextureHandle ?? 0,
            AlphaMode = 0,
            AlphaCutOff = 1.0f
        };

        pooledMaterial = new PooledMaterial(_pooledMaterials.Count);
        MaterialBuffer.Update(gpuMaterial, pooledMaterial.Index);

        _pooledMaterials.Add(material, pooledMaterial);
        return pooledMaterial;
    }
}