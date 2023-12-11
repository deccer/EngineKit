namespace EngineKit.Graphics;

public struct BufferClearInfo
{
    public BufferClearInfo()
    {
    }

    public int Offset = 0;

    public int Size = SizeInBytes.Whole;

    public uint Data = 0;
};