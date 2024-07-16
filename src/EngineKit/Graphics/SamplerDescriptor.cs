using EngineKit.Graphics.RHI;

namespace EngineKit.Graphics;

public struct SamplerDescriptor
{
    public Label Label;

    public TextureInterpolationFilter InterpolationFilter;

    public TextureMipmapFilter MipmapFilter;

    public float LodBias;

    public float MinLod;

    public float MaxLod;

    public TextureAddressMode TextureAddressModeU;

    public TextureAddressMode TextureAddressModeV;

    public TextureAddressMode TextureAddressModeW;

    public TextureSampleCount Anisotropy;

    public bool IsCompareEnabled;

    public CompareFunction CompareFunction;

    public TextureBorderColor TextureBorderColor;
}