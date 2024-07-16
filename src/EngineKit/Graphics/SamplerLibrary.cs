using System.Collections.Generic;
using EngineKit.Graphics.RHI;

namespace EngineKit.Graphics;

internal sealed class SamplerLibrary : ISamplerLibrary
{
    private readonly IGraphicsContext _graphicsContext;
    private IDictionary<SamplerInformation, ISampler> _samplers;

    public SamplerLibrary(IGraphicsContext graphicsContext)
    {
        _graphicsContext = graphicsContext;
        _samplers = new Dictionary<SamplerInformation, ISampler>();
    }

    public void Dispose()
    {
        foreach (var (_, sampler) in _samplers)
        {
            sampler.Dispose();
        }
    }

    public ISampler GetSampler(SamplerInformation samplerInformation)
    {
        if (_samplers.TryGetValue(samplerInformation, out var sampler))
        {
            return sampler;
        }

        throw new KeyNotFoundException();
    }

    public void AddSamplerIfNotExists(SamplerInformation? samplerInformation)
    {
        if (!samplerInformation.HasValue)
        {
            return;
        }

        if (_samplers.ContainsKey(samplerInformation.Value))
        {
            return;
        }

        var sampler = _graphicsContext.CreateSampler(new SamplerDescriptor
        {
            Anisotropy = TextureSampleCount.OneSample,
            InterpolationFilter = samplerInformation.Value.TextureInterpolationFilter,
            MipmapFilter = samplerInformation.Value.TextureMipmapFilter,
            TextureAddressModeU = samplerInformation.Value.TextureAddressingModeS,
            TextureAddressModeV = samplerInformation.Value.TextureAddressingModeT,
            MinLod = -1000.0f,
            MaxLod = 1000.0f,
            LodBias = 0.0f,
        });

        _samplers.Add(samplerInformation.Value, sampler);
    }
}
