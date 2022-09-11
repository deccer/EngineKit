using System;
using System.Buffers;
using EngineKit.Extensions;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

public class Texture : ITexture
{
    private readonly TextureCreateDescriptor _textureCreateDescriptor;
    private readonly uint _id;

    public Format Format
    {
        get => _textureCreateDescriptor.Format;
    }

    public ulong MakeResident()
    {
        var textureHandle = GL.GetTextureHandle(_id);
        GL.MakeTextureHandleResident(textureHandle);
        return textureHandle;
    }

    internal Texture(TextureCreateDescriptor textureCreateDescriptor)
    {
        _textureCreateDescriptor = textureCreateDescriptor;
        _id = GL.CreateTexture(_textureCreateDescriptor.ImageType.ToGL());

        if (!string.IsNullOrEmpty(textureCreateDescriptor.Label))
        {
            GL.ObjectLabel(GL.ObjectIdentifier.Texture, _id, textureCreateDescriptor.Label);
        }

        switch (textureCreateDescriptor.ImageType)
        {
            case ImageType.Texture1D:
                GL.TextureStorage1D(
                    _id,
                    textureCreateDescriptor.MipLevels,
                    textureCreateDescriptor.Format.ToSizedInternalFormat(),
                    textureCreateDescriptor.Size.X);
                break;
            case ImageType.Texture2D:
                GL.TextureStorage2D(
                    _id,
                    textureCreateDescriptor.MipLevels,
                    textureCreateDescriptor.Format.ToSizedInternalFormat(),
                    textureCreateDescriptor.Size.X,
                    textureCreateDescriptor.Size.Y);
                break;
            case ImageType.Texture3D:
                GL.TextureStorage3D(
                    _id,
                    textureCreateDescriptor.MipLevels,
                    textureCreateDescriptor.Format.ToSizedInternalFormat(),
                    textureCreateDescriptor.Size.X,
                    textureCreateDescriptor.Size.Y,
                    textureCreateDescriptor.Size.Z);
                break;
            case ImageType.Texture2DArray:
                GL.TextureStorage3D(
                    _id,
                    textureCreateDescriptor.MipLevels,
                    textureCreateDescriptor.Format.ToSizedInternalFormat(),
                    textureCreateDescriptor.Size.X,
                    textureCreateDescriptor.Size.Y,
                    (int)textureCreateDescriptor.ArrayLayers);
                break;
        }
    }

    public TextureView CreateTextureView()
    {
        var textureViewDescriptor = new TextureViewDescriptor
        {
            Format = _textureCreateDescriptor.Format,
            Label = _textureCreateDescriptor.Label + "_View",
            ImageType = _textureCreateDescriptor.ImageType,
            MinLayer = 0,
            NumLayers = _textureCreateDescriptor.ArrayLayers,
            MinLevel = 0,
            NumLevels = _textureCreateDescriptor.MipLevels
        };
        return new TextureView(textureViewDescriptor, this);
    }

    public uint Id => _id;

    public void Dispose()
    {
        GL.DeleteTexture(_id);
    }

    public void GenerateMipmaps()
    {
        GL.GenerateTextureMipmap(_id);
    }

    public void Upload(
        TextureUpdateDescriptor textureUpdateDescriptor,
        IntPtr pixelPtr)
    {
        switch (textureUpdateDescriptor.UploadDimension)
        {
            case UploadDimension.One:
                GL.TextureSubImage1D(
                    _id,
                    textureUpdateDescriptor.Level,
                    textureUpdateDescriptor.Offset.X,
                    textureUpdateDescriptor.Size.X,
                    textureUpdateDescriptor.UploadFormat.ToGL(),
                    textureUpdateDescriptor.UploadType.ToGL(),
                    pixelPtr);
                break;
            case UploadDimension.Two:
                GL.TextureSubImage2D(
                    _id,
                    textureUpdateDescriptor.Level,
                    textureUpdateDescriptor.Offset.X,
                    textureUpdateDescriptor.Offset.Y,
                    textureUpdateDescriptor.Size.X,
                    textureUpdateDescriptor.Size.Y,
                    textureUpdateDescriptor.UploadFormat.ToGL(),
                    textureUpdateDescriptor.UploadType.ToGL(),
                    pixelPtr);
                break;
            case UploadDimension.Three:
                GL.TextureSubImage3D(
                    _id,
                    textureUpdateDescriptor.Level,
                    textureUpdateDescriptor.Offset.X,
                    textureUpdateDescriptor.Offset.Y,
                    textureUpdateDescriptor.Offset.Z,
                    textureUpdateDescriptor.Size.X,
                    textureUpdateDescriptor.Size.Y,
                    textureUpdateDescriptor.Size.Z,
                    textureUpdateDescriptor.UploadFormat.ToGL(),
                    textureUpdateDescriptor.UploadType.ToGL(),
                    pixelPtr);
                break;
        }
    }

    public unsafe void Upload(
        TextureUpdateDescriptor textureUpdateDescriptor,
        MemoryHandle memoryHandle)
    {
        switch (textureUpdateDescriptor.UploadDimension)
        {
            case UploadDimension.One:
                GL.TextureSubImage1D(
                    _id,
                    textureUpdateDescriptor.Level,
                    textureUpdateDescriptor.Offset.X,
                    textureUpdateDescriptor.Size.X,
                    textureUpdateDescriptor.UploadFormat.ToGL(),
                    textureUpdateDescriptor.UploadType.ToGL(),
                    memoryHandle.Pointer);
                break;
            case UploadDimension.Two:
                GL.TextureSubImage2D(
                    _id,
                    textureUpdateDescriptor.Level,
                    textureUpdateDescriptor.Offset.X,
                    textureUpdateDescriptor.Offset.Y,
                    textureUpdateDescriptor.Size.X,
                    textureUpdateDescriptor.Size.Y,
                    textureUpdateDescriptor.UploadFormat.ToGL(),
                    textureUpdateDescriptor.UploadType.ToGL(),
                    memoryHandle.Pointer);
                break;
            case UploadDimension.Three:
                GL.TextureSubImage3D(
                    _id,
                    textureUpdateDescriptor.Level,
                    textureUpdateDescriptor.Offset.X,
                    textureUpdateDescriptor.Offset.Y,
                    textureUpdateDescriptor.Offset.Z,
                    textureUpdateDescriptor.Size.X,
                    textureUpdateDescriptor.Size.Y,
                    textureUpdateDescriptor.Size.Z,
                    textureUpdateDescriptor.UploadFormat.ToGL(),
                    textureUpdateDescriptor.UploadType.ToGL(),
                    memoryHandle.Pointer);
                break;
        }
    }

    public void Upload<TPixel>(
        TextureUpdateDescriptor textureUpdateDescriptor,
        in TPixel pixelData) where TPixel : unmanaged
    {
        switch (textureUpdateDescriptor.UploadDimension)
        {
            case UploadDimension.One:
                GL.TextureSubImage1D(
                    _id,
                    textureUpdateDescriptor.Level,
                    textureUpdateDescriptor.Offset.X,
                    textureUpdateDescriptor.Size.X,
                    textureUpdateDescriptor.UploadFormat.ToGL(),
                    textureUpdateDescriptor.UploadType.ToGL(),
                    pixelData);
                break;
            case UploadDimension.Two:
                GL.TextureSubImage2D(
                    _id,
                    textureUpdateDescriptor.Level,
                    textureUpdateDescriptor.Offset.X,
                    textureUpdateDescriptor.Offset.Y,
                    textureUpdateDescriptor.Size.X,
                    textureUpdateDescriptor.Size.Y,
                    textureUpdateDescriptor.UploadFormat.ToGL(),
                    textureUpdateDescriptor.UploadType.ToGL(),
                    pixelData);
                break;
            case UploadDimension.Three:
                GL.TextureSubImage3D(
                    _id,
                    textureUpdateDescriptor.Level,
                    textureUpdateDescriptor.Offset.X,
                    textureUpdateDescriptor.Offset.Y,
                    textureUpdateDescriptor.Offset.Z,
                    textureUpdateDescriptor.Size.X,
                    textureUpdateDescriptor.Size.Y,
                    textureUpdateDescriptor.Size.Z,
                    textureUpdateDescriptor.UploadFormat.ToGL(),
                    textureUpdateDescriptor.UploadType.ToGL(),
                    pixelData);
                break;
        }
    }
}