using System;
using EngineKit.Extensions;
using EngineKit.Graphics.RHI;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

internal sealed class Sampler : ISampler
{
    private static readonly int[] _transparentBorderColorInt = { 0, 0, 0, 0 };
    private static readonly float[] _transparentBorderColorFloat = { 0.0f, 0.0f, 0.0f, 0.0f };
    private static readonly int[] _whiteBorderColorInt = { 1, 1, 1, 1 };
    private static readonly float[] _whiteBorderColorFloat = { 1.0f, 1.0f, 1.0f, 1.0f };
    private static readonly int[] _blackBorderColorInt = { 0, 0, 0, 1 };
    private static readonly float[] _blackBorderColorFloat = { 0.0f, 0.0f, 0.0f, 1.0f };
    private readonly uint _id;

    public uint Id => _id;

    public Sampler(SamplerDescriptor samplerDescriptor)
    {
        _id = GL.CreateSampler();
        if (!string.IsNullOrEmpty(samplerDescriptor.Label))
        {
            GL.ObjectLabel(GL.ObjectIdentifier.Sampler, _id, $"Sampler-{samplerDescriptor.Label}");
        }

        GL.SamplerParameter(_id, GL.SamplerParameterI.TextureMagFilter, (int)samplerDescriptor.InterpolationFilter.ToGL());
        GL.SamplerParameter(_id, GL.SamplerParameterI.TextureMinFilter, (int)samplerDescriptor.MipmapFilter.ToGL());

        GL.SamplerParameter(_id, GL.SamplerParameterI.TextureWrapS, (int)samplerDescriptor.TextureAddressModeU.ToGL());
        GL.SamplerParameter(_id, GL.SamplerParameterI.TextureWrapT, (int)samplerDescriptor.TextureAddressModeV.ToGL());
        GL.SamplerParameter(_id, GL.SamplerParameterI.TextureWrapR, (int)samplerDescriptor.TextureAddressModeW.ToGL());

        if (samplerDescriptor.TextureAddressModeU == TextureAddressMode.ClampToEdge ||
            samplerDescriptor.TextureAddressModeV == TextureAddressMode.ClampToEdge ||
            samplerDescriptor.TextureAddressModeW == TextureAddressMode.ClampToEdge)
        {
            switch (samplerDescriptor.TextureBorderColor)
            {
                case TextureBorderColor.FloatOpaqueBlack:
                    GL.SamplerParameter(_id, GL.SamplerParameterF.TextureBorderColor, _blackBorderColorFloat);
                    break;
                case TextureBorderColor.FloatTransparentBlack:
                    GL.SamplerParameter(_id, GL.SamplerParameterF.TextureBorderColor, _transparentBorderColorFloat);
                    break;
                case TextureBorderColor.FloatOpaqueWhite:
                    GL.SamplerParameter(_id, GL.SamplerParameterF.TextureBorderColor, _whiteBorderColorFloat);
                    break;
                /*
                case BorderColor.IntTransparentBlack:
                    GL.SamplerParameter(_id, GL.SamplerParameterF.TextureBorderColor, _transparentBorderColorInt);
                    break;
                case BorderColor.IntOpaqueBlack:
                    GL.SamplerParameter(_id, GL.SamplerParameterF.TextureBorderColor, _blackBorderColorInt);
                    break;
                case BorderColor.IntOpaqueWhite:
                    GL.SamplerParameter(_id, GL.SamplerParameterF.TextureBorderColor, _whiteBorderColorInt);
                    break;
                    */
            }
        }

        GL.SamplerParameter(_id, GL.SamplerParameterF.TextureMaxAnisotropy, ToAnistropy(samplerDescriptor.Anisotropy));
        GL.SamplerParameter(_id, GL.SamplerParameterF.TextureLodBias, samplerDescriptor.LodBias);
        GL.SamplerParameter(_id, GL.SamplerParameterF.TextureMinLod, samplerDescriptor.MinLod);
        GL.SamplerParameter(_id, GL.SamplerParameterF.TextureMaxLod, samplerDescriptor.MaxLod);

        GL.SamplerParameter(_id, GL.SamplerParameterI.TextureCompareMode, samplerDescriptor.IsCompareEnabled
            ? (int)GL.TextureCompareMode.CompareRefToTexture
            : (int)GL.TextureCompareMode.None);
        if (samplerDescriptor.IsCompareEnabled)
        {
            GL.SamplerParameter(_id, GL.SamplerParameterI.TextureCompareFunc, (int)samplerDescriptor.CompareFunction);
        }
    }

    public void Dispose()
    {
        GL.DeleteSampler(_id);
    }

    private float ToAnistropy(TextureSampleCount textureSampleCount)
    {
        return textureSampleCount switch
        {
            TextureSampleCount.OneSample => 1.0f,
            TextureSampleCount.TwoSamples => 2.0f,
            TextureSampleCount.FourSamples => 4.0f,
            TextureSampleCount.EightSamples => 8.0f,
            TextureSampleCount.SixteenSamples => 16.0f,
            _ => throw new ArgumentOutOfRangeException(nameof(textureSampleCount), textureSampleCount, null)
        };
    }
}