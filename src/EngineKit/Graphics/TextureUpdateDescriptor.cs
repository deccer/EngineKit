using OpenTK.Mathematics;

namespace EngineKit.Graphics;

public record struct TextureUpdateDescriptor
{
    public UploadDimension UploadDimension;

    public int Level;

    public Vector3i Offset;

    public Vector3i Size;

    public UploadFormat UploadFormat;

    public UploadType UploadType;
}