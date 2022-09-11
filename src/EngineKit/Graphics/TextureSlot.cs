namespace EngineKit.Graphics;

internal readonly struct TextureSlot
{
    public readonly int ArraySlice;
    public readonly int TextureArrayIndex;

    public TextureSlot(int textureArrayIndex, int arraySlice)
    {
        ArraySlice = arraySlice;
        TextureArrayIndex = textureArrayIndex;
    }
}