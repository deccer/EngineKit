namespace EngineKit.Graphics;

public class TextureId
{
    public TextureId(int arrayIndex, int arraySlice)
    {
        ArrayIndex = arrayIndex;
        ArraySlice = arraySlice;
    }

    public int ArrayIndex { get; }

    public int ArraySlice { get; }
}