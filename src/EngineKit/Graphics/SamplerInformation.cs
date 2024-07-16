using System;
using System.Diagnostics;
using EngineKit.Extensions;
using EngineKit.Graphics.RHI;

namespace EngineKit.Graphics;

[DebuggerDisplay("S = {TextureAddressingModeS}, T = {TextureAddressingModeT}, IF = {TextureInterpolationFilter}, MF = {TextureMipmapFilter}")]
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

    public override string ToString()
    {
        return $"S = {TextureAddressingModeS}, T = {TextureAddressingModeT}, IF = {TextureInterpolationFilter}, MF = {TextureMipmapFilter}";
    }

    public bool Equals(SamplerInformation other)
    {
        return TextureAddressingModeS == other.TextureAddressingModeS &&
               TextureAddressingModeT == other.TextureAddressingModeT &&
               TextureInterpolationFilter == other.TextureInterpolationFilter &&
               TextureMipmapFilter == other.TextureMipmapFilter;
    }

    public override bool Equals(object? obj)
    {
        return obj is SamplerInformation other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)TextureAddressingModeS, (int)TextureAddressingModeT, (int)TextureInterpolationFilter, (int)TextureMipmapFilter);
    }

    public static bool operator ==(SamplerInformation left, SamplerInformation right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SamplerInformation left, SamplerInformation right)
    {
        return !(left == right);
    }
}
