namespace EngineKit.Graphics;

internal readonly struct MaterialTextureSlot
{
    public readonly string MaterialName;

    public readonly TextureSlot BaseColorTextureSlot;

    public readonly TextureSlot NormalTextureSlot;

    public readonly TextureSlot SpecularTextureSlot;

    public MaterialTextureSlot(
        string materialName,
        TextureSlot baseColorTextureSlot,
        TextureSlot normalTextureSlot,
        TextureSlot specularTextureSlot)
    {
        MaterialName = materialName;
        BaseColorTextureSlot = baseColorTextureSlot;
        NormalTextureSlot = normalTextureSlot;
        SpecularTextureSlot = specularTextureSlot;
    }
}