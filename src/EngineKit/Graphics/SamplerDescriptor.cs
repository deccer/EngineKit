namespace EngineKit.Graphics;

public struct SamplerDescriptor
{
    public Filter MinFilter;
    public Filter MagFilter;
    public float LodBias;
    public float MinLod;
    public float MaxLod;
    public AddressMode AddressModeU;
    public AddressMode AddressModeV;
    public SampleCount Anisotropy;
    public bool IsCompareEnabled;
    public CompareOperation CompareOperation;
}