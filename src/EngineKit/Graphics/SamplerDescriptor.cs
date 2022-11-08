namespace EngineKit.Graphics;

public struct SamplerDescriptor
{
    public Filter MinFilter;
    public Filter MagFilter;
    public Filter MipmapFilter;
    public float LodBias;
    public float MinLod;
    public float MaxLod;
    public AddressMode AddressModeU;
    public AddressMode AddressModeV;
    public AddressMode AddressModeW;
    public SampleCount Anisotropy;
    public bool IsCompareEnabled;
    public CompareOperation CompareOperation;
    public BorderColor BorderColor;
}