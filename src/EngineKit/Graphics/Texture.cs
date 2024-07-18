using System;
using System.Buffers;
using EngineKit.Extensions;
using EngineKit.Graphics.RHI;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

public class Texture : ITexture
{
    private readonly TextureCreateDescriptor _textureCreateDescriptor;
    private readonly uint _id;
    private bool _isResident;

    public TextureCreateDescriptor TextureCreateDescriptor => _textureCreateDescriptor;

    public Format Format => _textureCreateDescriptor.Format;

    public ulong TextureHandle { get; private set; }

    public void MakeResident()
    {
        TextureHandle = GL.GetTextureHandle(_id);
        GL.MakeTextureHandleResident(TextureHandle);
        _isResident = true;
    }
    
    public void MakeResident(ISampler sampler)
    {
        TextureHandle = GL.GetTextureSamplerHandle(_id, sampler.Id);
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
        _id = GL.CreateTexture(_textureCreateDescriptor.TextureType.ToGL());

        if (!string.IsNullOrEmpty(textureCreateDescriptor.Label))
        {
            GL.ObjectLabel(GL.ObjectIdentifier.Texture, _id, textureCreateDescriptor.Label);
        }

        switch (textureCreateDescriptor.TextureType)
        {
            case TextureType.Texture1D:
                GL.TextureStorage1D(
                    _id,
                    textureCreateDescriptor.MipLevels,
                    textureCreateDescriptor.Format.ToGL(),
                    textureCreateDescriptor.Size.X);
                break;
            case TextureType.Texture2D:
                GL.TextureStorage2D(
                    _id,
                    textureCreateDescriptor.MipLevels,
                    textureCreateDescriptor.Format.ToGL(),
                    textureCreateDescriptor.Size.X,
                    textureCreateDescriptor.Size.Y);
                break;
            case TextureType.TextureCube:
                GL.TextureStorage2D(
                    _id,
                    textureCreateDescriptor.MipLevels,
                    textureCreateDescriptor.Format.ToGL(),
                    textureCreateDescriptor.Size.X,
                    textureCreateDescriptor.Size.Y);
                break;
            case TextureType.Texture3D:
                GL.TextureStorage3D(
                    _id,
                    textureCreateDescriptor.MipLevels,
                    textureCreateDescriptor.Format.ToGL(),
                    textureCreateDescriptor.Size.X,
                    textureCreateDescriptor.Size.Y,
                    textureCreateDescriptor.Size.Z);
                break;
            case TextureType.Texture2DArray:
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
                    $"ImageType {textureCreateDescriptor.TextureType} is not implemented yet");
        }
    }

    public TextureView CreateTextureView()
    {
        var textureViewDescriptor = new TextureViewDescriptor
        {
            Format = _textureCreateDescriptor.Format,
            Label = _textureCreateDescriptor.Label + "-View",
            TextureType = _textureCreateDescriptor.TextureType,
            MinLayer = 0,
            NumLayers = _textureCreateDescriptor.ArrayLayers + 1,
            MinLevel = 0,
            NumLevels = _textureCreateDescriptor.MipLevels
        };
        return new TextureView(textureViewDescriptor, this);
    }
    
    public TextureView CreateTextureView(SwizzleMapping swizzleMapping)
    {
        var textureViewDescriptor = new TextureViewDescriptor
        {
            Format = _textureCreateDescriptor.Format,
            Label = _textureCreateDescriptor.Label + "-View",
            TextureType = _textureCreateDescriptor.TextureType,
            MinLayer = 0,
            NumLayers = _textureCreateDescriptor.ArrayLayers + 1,
            MinLevel = 0,
            NumLevels = _textureCreateDescriptor.MipLevels,
            SwizzleMapping = swizzleMapping
        };
        return new TextureView(textureViewDescriptor, this);
    }
    
    public TextureView CreateTextureView(TextureViewDescriptor textureViewDescriptor)
    {
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
        nint pixelPtr)
    {
        if (TextureCreateDescriptor.Format.IsCompressedFormat())
        {
            UpdateCompressed(textureUpdateDescriptor, pixelPtr);
        }
        else
        {
            UpdateUncompressed(textureUpdateDescriptor, pixelPtr);
        }
    }

    public void Update(
        TextureUpdateDescriptor textureUpdateDescriptor,
        MemoryHandle memoryHandle)
    {
        if (TextureCreateDescriptor.Format.IsCompressedFormat())
        {
            UpdateCompressed(textureUpdateDescriptor, memoryHandle);
        }
        else
        {
            UpdateUncompressed(textureUpdateDescriptor, memoryHandle);
        }
    }

    public void Update<TPixel>(
        TextureUpdateDescriptor textureUpdateDescriptor,
        in TPixel pixelData) where TPixel : unmanaged
    {
        if (TextureCreateDescriptor.Format.IsCompressedFormat())
        {
            UpdateCompressed(textureUpdateDescriptor, pixelData);
        }
        else
        {
            UpdateUncompressed(textureUpdateDescriptor, pixelData);
        }
    }

    private void UpdateUncompressed<TPixel>(TextureUpdateDescriptor textureUpdateDescriptor, in TPixel pixelData)
        where TPixel : unmanaged
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
        
    private unsafe void UpdateUncompressed(TextureUpdateDescriptor textureUpdateDescriptor, MemoryHandle memoryHandle)
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

    private void UpdateUncompressed(TextureUpdateDescriptor textureUpdateDescriptor, nint pixelPtr)
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

    private unsafe void UpdateCompressed(TextureUpdateDescriptor textureUpdateDescriptor, MemoryHandle memoryHandle)
    {
        var compressedImageSize = GetBlockCompressedImageSize(
            TextureCreateDescriptor.Format,
            textureUpdateDescriptor.Size.X,
            textureUpdateDescriptor.Size.Y,
            textureUpdateDescriptor.Size.Z);
        GL.CompressedTextureSubImage2D(
            Id,
            textureUpdateDescriptor.Level,
            textureUpdateDescriptor.Offset.X,
            textureUpdateDescriptor.Offset.Y,
            textureUpdateDescriptor.Size.X,
            textureUpdateDescriptor.Size.Y,
            TextureCreateDescriptor.Format,
            compressedImageSize,
            //textureUpdateDescriptor.Offset.Z,
            memoryHandle.Pointer);
    }

    private unsafe void UpdateCompressed<TPixel>(TextureUpdateDescriptor textureUpdateDescriptor, in TPixel pixelData)
        where TPixel : unmanaged
    {
        var compressedImageSize = GetBlockCompressedImageSize(
            TextureCreateDescriptor.Format,
            textureUpdateDescriptor.Size.X,
            textureUpdateDescriptor.Size.Y,
            textureUpdateDescriptor.Size.Z);
        fixed (void* pixelDataPtr = &pixelData)
        {
            GL.CompressedTextureSubImage2D(
                Id,
                textureUpdateDescriptor.Level,
                textureUpdateDescriptor.Offset.X,
                textureUpdateDescriptor.Offset.Y,
                textureUpdateDescriptor.Size.X,
                textureUpdateDescriptor.Size.Y,
                TextureCreateDescriptor.Format,
                compressedImageSize,
                pixelDataPtr);
        }
    }

    private unsafe void UpdateCompressed(TextureUpdateDescriptor textureUpdateDescriptor, nint pixelPtr)
    {
        var compressedImageSize = GetBlockCompressedImageSize(
            TextureCreateDescriptor.Format,
            textureUpdateDescriptor.Size.X,
            textureUpdateDescriptor.Size.Y,
            textureUpdateDescriptor.Size.Z);
        GL.CompressedTextureSubImage2D(
            Id,
            textureUpdateDescriptor.Level,
            textureUpdateDescriptor.Offset.X,
            textureUpdateDescriptor.Offset.Y,
            textureUpdateDescriptor.Size.X,
            textureUpdateDescriptor.Size.Y,
            TextureCreateDescriptor.Format,
            compressedImageSize,
            (void*)pixelPtr);
    }

    private static long GetBlockCompressedImageSize(Format format, int width, int height, int depth)
    {
        width = (width + 4 - 1) & -4;
        height = (height + 4 - 1) & -4;

        switch (format)
        {
            // BC1 and BC4 store 4x4 blocks with 64 bits (8 bytes)
            case Format.Bc1RgbUNorm:
            case Format.Bc1RgbaUNorm:
            case Format.Bc1RgbSrgb:
            case Format.Bc1RgbaSrgb:
            case Format.Bc4RSNorm:
            case Format.Bc4RUNorm:
                return width * height * depth / 2;

            // BC3, BC5, BC6, and BC7 store 4x4 blocks with 128 bits (16 bytes)
            case Format.Bc2RgbaUNorm:
            case Format.Bc2RgbaSrgb:
            case Format.Bc3RgbaUNorm:
            case Format.Bc3RgbaSrgb:
            case Format.Bc5RgSNorm:
            case Format.Bc5RgUNorm:
            case Format.Bc6hRgbSFloat:
            case Format.Bc6hRgbUFloat:
            case Format.Bc7RgbaUNorm:
            case Format.Bc7RgbaSrgb:
                return width * height * depth;
            default:
                return 0;
        }
    }
}