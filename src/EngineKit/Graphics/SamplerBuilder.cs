using EngineKit.Core;
using EngineKit.Graphics.RHI;

namespace EngineKit.Graphics;

public class SamplerBuilder
{
    private readonly IGraphicsContext _graphicsContext;
    private SamplerDescriptor _samplerDescriptor;

    public SamplerBuilder(IGraphicsContext graphicsContext)
    {
        _graphicsContext = graphicsContext;
        _samplerDescriptor = new SamplerDescriptor();
        _samplerDescriptor.LodBias = 0.0f;
        _samplerDescriptor.MinLod = -1000.0f;
        _samplerDescriptor.MaxLod = 1000.0f;
        _samplerDescriptor.Anisotropy = TextureSampleCount.OneSample;
        _samplerDescriptor.CompareFunction = CompareFunction.Less;
        _samplerDescriptor.IsCompareEnabled = false;
    }

    public SamplerBuilder WithInterpolationFilter(TextureInterpolationFilter filter)
    {
        _samplerDescriptor.InterpolationFilter = filter;
        return this;
    }

    public SamplerBuilder WithMipmapFilter(TextureMipmapFilter mipmapFilter)
    {
        _samplerDescriptor.MipmapFilter = mipmapFilter;
        return this;
    }

    public SamplerBuilder WithLodBias(float loadBias)
    {
        _samplerDescriptor.LodBias = loadBias;
        return this;
    }

    public SamplerBuilder WithLod(float minLod, float maxLod)
    {
        _samplerDescriptor.MinLod = minLod;
        _samplerDescriptor.MaxLod = maxLod;
        return this;
    }

    public SamplerBuilder WithAddressMode(TextureAddressMode textureAddressMode)
    {
        _samplerDescriptor.TextureAddressModeU = textureAddressMode;
        _samplerDescriptor.TextureAddressModeV = textureAddressMode;
        _samplerDescriptor.TextureAddressModeW = textureAddressMode;
        return this;
    }

    public ISampler Build(Label label)
    {
        _samplerDescriptor.Label = label;
        return _graphicsContext.CreateSampler(_samplerDescriptor);
    }
}
