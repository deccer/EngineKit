namespace EngineKit.Graphics;

public struct BufferClearInfo
{
    public BufferClearInfo()
    {
    }

    public uint Offset = 0;

    public uint Size = SizeInBytes.Whole;

    public uint Value = 0;
};