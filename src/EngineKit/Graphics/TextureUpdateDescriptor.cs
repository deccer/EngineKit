using EngineKit.Graphics.RHI;
using EngineKit.Mathematics;

namespace EngineKit.Graphics;

public record struct TextureUpdateDescriptor
{
    public UploadDimension UploadDimension;

    public int Level;

    public Int3 Offset;

    public Int3 Size;

    public UploadFormat UploadFormat;

    public UploadType UploadType;
}