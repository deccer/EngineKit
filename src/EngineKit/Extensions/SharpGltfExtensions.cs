using System;
using EngineKit.Graphics;
using SharpGLTF.Schema2;
using TextureInterpolationFilter = EngineKit.Graphics.TextureInterpolationFilter;

namespace EngineKit.Extensions;

public static class SharpGltfExtensions
{
    public static TextureAddressMode ToAddressingMode(this SharpGLTF.Schema2.TextureWrapMode textureWrapMode)
    {
        return textureWrapMode switch
        {
            SharpGLTF.Schema2.TextureWrapMode.REPEAT => TextureAddressMode.Repeat,
            SharpGLTF.Schema2.TextureWrapMode.CLAMP_TO_EDGE => TextureAddressMode.ClampToEdge,
            SharpGLTF.Schema2.TextureWrapMode.MIRRORED_REPEAT => TextureAddressMode.MirroredRepeat,
            _ => throw new ArgumentOutOfRangeException(nameof(textureWrapMode), textureWrapMode, null)
        };
    }

    public static TextureInterpolationFilter ToInterpolationFilter(
        this SharpGLTF.Schema2.TextureInterpolationFilter interpolationFilter)
    {
        return interpolationFilter switch
        {
            SharpGLTF.Schema2.TextureInterpolationFilter.LINEAR => TextureInterpolationFilter.Linear,
            SharpGLTF.Schema2.TextureInterpolationFilter.DEFAULT => TextureInterpolationFilter.Default,
            SharpGLTF.Schema2.TextureInterpolationFilter.NEAREST => TextureInterpolationFilter.Nearest,
            _ => throw new ArgumentOutOfRangeException(nameof(interpolationFilter), interpolationFilter, null)
        };
    }

    public static TextureMipmapFilter ToMipmapFilter(this SharpGLTF.Schema2.TextureMipMapFilter mipMapFilter)
    {
        return mipMapFilter switch
        {
            TextureMipMapFilter.NEAREST => TextureMipmapFilter.Nearest,
            TextureMipMapFilter.LINEAR => TextureMipmapFilter.Linear,
            TextureMipMapFilter.NEAREST_MIPMAP_NEAREST => TextureMipmapFilter.NearestMipmapNearest,
            TextureMipMapFilter.LINEAR_MIPMAP_NEAREST => TextureMipmapFilter.LinearMipmapNearest,
            TextureMipMapFilter.NEAREST_MIPMAP_LINEAR => TextureMipmapFilter.NearestMipmapLinear,
            TextureMipMapFilter.LINEAR_MIPMAP_LINEAR => TextureMipmapFilter.LinearMipmapLinear,
            TextureMipMapFilter.DEFAULT => TextureMipmapFilter.Default,
            _ => throw new ArgumentOutOfRangeException(nameof(mipMapFilter), mipMapFilter, null)
        };
    }
}