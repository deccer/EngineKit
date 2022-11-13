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
        _samplerDescriptor.Anisotropy = SampleCount.OneSample;
        _samplerDescriptor.CompareOperation = CompareOperation.Less;
        _samplerDescriptor.IsCompareEnabled = false;
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

    public SamplerBuilder WithAddressMode(AddressMode addressMode)
    {
        _samplerDescriptor.AddressModeU = addressMode;
        _samplerDescriptor.AddressModeV = addressMode;
        _samplerDescriptor.AddressModeW = addressMode;
        return this;
    }

    public ISampler Build(Label label)
    {
        _samplerDescriptor.Label = label;
        return _graphicsContext.CreateSampler(_samplerDescriptor);
    }
}