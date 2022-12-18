using System;
using EngineKit.Extensions;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

public class TextureView : IDisposable
{
    private readonly uint _id;

    internal TextureView(
        TextureViewDescriptor textureViewDescriptor,
        ITexture texture)
    {
        _id = GL.GenTexture();
        Format = textureViewDescriptor.Format;
        GL.TextureView(
            _id,
            textureViewDescriptor.ImageType.ToGL(),
            texture.Id,
            textureViewDescriptor.Format.ToGL(),
            textureViewDescriptor.MinLayer,
            textureViewDescriptor.NumLayers,
            textureViewDescriptor.MinLevel,
            textureViewDescriptor.NumLevels);

        if (!string.IsNullOrEmpty(textureViewDescriptor.Label))
        {
            GL.ObjectLabel(GL.ObjectIdentifier.Texture, _id, textureViewDescriptor.Label);
        }
    }

    public uint Id => _id;

    public Format Format { get; }

    public void Dispose()
    {
        GL.DeleteTexture(_id);
    }
}