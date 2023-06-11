using System;
using EngineKit.Extensions;

namespace EngineKit.Graphics;

public readonly struct SamplerInformation : IEquatable<SamplerInformation>
{
    public SamplerInformation(SharpGLTF.Schema2.TextureSampler textureSampler)
    {
        TextureAddressingModeS = textureSampler.WrapS.ToAddressingMode();
        TextureAddressingModeT = textureSampler.WrapT.ToAddressingMode();
        TextureInterpolationFilter = textureSampler.MagFilter.ToInterpolationFilter();
        TextureMipmapFilter = textureSampler.MinFilter.ToMipmapFilter();
    }
    
    public readonly TextureAddressMode TextureAddressingModeS;
    public readonly TextureAddressMode TextureAddressingModeT;
    public readonly TextureInterpolationFilter TextureInterpolationFilter;
    public readonly TextureMipmapFilter TextureMipmapFilter;

    public bool Equals(SamplerInformation other)
    {
        return TextureAddressingModeS == other.TextureAddressingModeS && TextureAddressingModeT == other.TextureAddressingModeT && TextureInterpolationFilter == other.TextureInterpolationFilter && TextureMipmapFilter == other.TextureMipmapFilter;
    }

    public override bool Equals(object? obj)
    {
        return obj is SamplerInformation other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)TextureAddressingModeS, (int)TextureAddressingModeT, (int)TextureInterpolationFilter, (int)TextureMipmapFilter);
    }
}