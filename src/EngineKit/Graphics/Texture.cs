using System;
using System.Buffers;
using EngineKit.Extensions;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

public class Texture : ITexture
{
    private readonly TextureCreateDescriptor _textureCreateDescriptor;
    private readonly uint _id;
    private bool _isResident;

    public Format Format
    {
        get => _textureCreateDescriptor.Format;
    }

    public ulong TextureHandle { get; private set; }

    public void MakeResident()
    {
        TextureHandle = GL.GetTextureHandle(_id);
        GL.MakeTextureHandleResident(TextureHandle);
        _isResident = true;
    }

    public void MakeNonResident()
    {
        if (!_isResident)
        {
            return;
        }

        GL.MakeTextureHandleNonResident(TextureHandle);
        _isResident = false;
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
                    textureCreateDescriptor.Format.ToGL(),
                    textureCreateDescriptor.Size.X);
                break;
            case ImageType.Texture2D:
                GL.TextureStorage2D(
                    _id,
                    textureCreateDescriptor.MipLevels,
                    textureCreateDescriptor.Format.ToGL(),
                    textureCreateDescriptor.Size.X,
                    textureCreateDescriptor.Size.Y);
                break;
            case ImageType.TextureCube:
                GL.TextureStorage2D(
                    _id,
                    textureCreateDescriptor.MipLevels,
                    textureCreateDescriptor.Format.ToGL(),
                    textureCreateDescriptor.Size.X,
                    textureCreateDescriptor.Size.Y);
                break;
            case ImageType.Texture3D:
                GL.TextureStorage3D(
                    _id,
                    textureCreateDescriptor.MipLevels,
                    textureCreateDescriptor.Format.ToGL(),
                    textureCreateDescriptor.Size.X,
                    textureCreateDescriptor.Size.Y,
                    textureCreateDescriptor.Size.Z);
                break;
            case ImageType.Texture2DArray:
                GL.TextureStorage3D(
                    _id,
                    textureCreateDescriptor.MipLevels,
                    textureCreateDescriptor.Format.ToGL(),
                    textureCreateDescriptor.Size.X,
                    textureCreateDescriptor.Size.Y,
                    (int)textureCreateDescriptor.ArrayLayers);
                break;
            default:
                throw new NotImplementedException(
                    $"ImageType {textureCreateDescriptor.ImageType} is not implemented yet");
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
        MakeNonResident();
        GL.DeleteTexture(_id);
    }

    public void GenerateMipmaps()
    {
        GL.GenerateTextureMipmap(_id);
    }

    public void Update(
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

    public unsafe void Update(
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

    public void Update<TPixel>(
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