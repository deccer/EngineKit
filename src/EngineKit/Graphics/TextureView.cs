using System;
using EngineKit.Extensions;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

public class TextureView : IHasTextureId, IDisposable 
{
    private readonly uint _id;

    internal TextureView(
        TextureViewDescriptor textureViewDescriptor,
        ITexture texture)
    {
        _id = GL.GenTexture();
        Format = textureViewDescriptor.Format;
        Width = texture.TextureCreateDescriptor.Size.X;
        Height = texture.TextureCreateDescriptor.Size.Y;
        Depth = texture.TextureCreateDescriptor.Size.Z;
        GL.TextureView(
            _id,
            textureViewDescriptor.ImageType.ToGL(),
            texture.Id,
            textureViewDescriptor.Format.ToGL(),
            textureViewDescriptor.MinLevel,
            textureViewDescriptor.NumLevels,
            textureViewDescriptor.MinLayer,
            textureViewDescriptor.NumLayers
        );
        
        GL.TextureParameter(_id, GL.TextureParameterName.TextureSwizzleR, textureViewDescriptor.SwizzleMapping.Red.ToGL());
        GL.TextureParameter(_id, GL.TextureParameterName.TextureSwizzleG, textureViewDescriptor.SwizzleMapping.Green.ToGL());
        GL.TextureParameter(_id, GL.TextureParameterName.TextureSwizzleB, textureViewDescriptor.SwizzleMapping.Blue.ToGL());
        GL.TextureParameter(_id, GL.TextureParameterName.TextureSwizzleA, textureViewDescriptor.SwizzleMapping.Alpha.ToGL());

        if (!string.IsNullOrEmpty(textureViewDescriptor.Label))
        {
            GL.ObjectLabel(GL.ObjectIdentifier.Texture, _id, textureViewDescriptor.Label);
        }
    }

    public uint Id => _id;

    public Format Format { get; }
    
    public int Width { get; private set; }
    
    public int Height { get; private set; }
    
    public int Depth { get; private set; }

    public void Dispose()
    {
        GL.DeleteTexture(_id);
    }
}