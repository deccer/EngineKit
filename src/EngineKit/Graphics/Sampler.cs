using System;
using EngineKit.Extensions;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

public sealed class Sampler : IDisposable
{
    private static readonly int[] _transparentBorderColorInt = { 0, 0, 0, 0 };
    private static readonly float[] _transparentBorderColorFloat = { 0.0f, 0.0f, 0.0f, 0.0f };
    private static readonly int[] _whiteBorderColorInt = { 1, 1, 1, 1 };
    private static readonly float[] _whiteBorderColorFloat = { 1.0f, 1.0f, 1.0f, 1.0f };
    private static readonly int[] _blackBorderColorInt = { 0, 0, 0, 1 };
    private static readonly float[] _blackBorderColorFloat = { 0.0f, 0.0f, 0.0f, 1.0f };
    private readonly uint _id;

    public uint Id => _id;

    public static Sampler Create(SamplerDescriptor samplerDescriptor)
    {
        var sampler = new Sampler(samplerDescriptor);
        return sampler;
    }

    private Sampler(SamplerDescriptor samplerDescriptor)
    {
        _id = GL.CreateSampler();

        var minFilter = GL.Filter.Nearest;
        GL.SamplerParameter(_id, GL.SamplerParameterI.TextureMagFilter, (int)samplerDescriptor.MagFilter.ToGL());
        switch (samplerDescriptor.MipmapFilter)
        {
            case Filter.None:
                minFilter = samplerDescriptor.MinFilter == Filter.Linear
                    ? GL.Filter.Linear
                    : GL.Filter.Nearest;
                break;
            case Filter.Nearest:
                minFilter = samplerDescriptor.MinFilter == Filter.Linear
                    ? GL.Filter.LinearMipmapNearest
                    : GL.Filter.NearestMipmapNearest;
                break;
            case Filter.Linear:
                minFilter = samplerDescriptor.MinFilter == Filter.Linear
                    ? GL.Filter.LinearMipmapLinear
                    : GL.Filter.NearestMipmapLinear;
                break;
        }
        GL.SamplerParameter(_id, GL.SamplerParameterI.TextureMinFilter, (int)minFilter);

        GL.SamplerParameter(_id, GL.SamplerParameterI.TextureWrapS, (int)samplerDescriptor.AddressModeU.ToGL());
        GL.SamplerParameter(_id, GL.SamplerParameterI.TextureWrapT, (int)samplerDescriptor.AddressModeV.ToGL());
        GL.SamplerParameter(_id, GL.SamplerParameterI.TextureWrapR, (int)samplerDescriptor.AddressModeW.ToGL());

        if (samplerDescriptor.AddressModeU == AddressMode.ClampToEdge ||
            samplerDescriptor.AddressModeV == AddressMode.ClampToEdge ||
            samplerDescriptor.AddressModeW == AddressMode.ClampToEdge)
        {
            switch (samplerDescriptor.BorderColor)
            {
                case BorderColor.FloatOpaqueBlack:
                    GL.SamplerParameter(_id, GL.SamplerParameterF.TextureBorderColor, _blackBorderColorFloat);
                    break;
                case BorderColor.FloatTransparentBlack:
                    GL.SamplerParameter(_id, GL.SamplerParameterF.TextureBorderColor, _transparentBorderColorFloat);
                    break;
                case BorderColor.FloatOpaqueWhite:
                    GL.SamplerParameter(_id, GL.SamplerParameterF.TextureBorderColor, _whiteBorderColorFloat);
                    break;
                case BorderColor.IntTransparentBlack:
                    GL.SamplerParameter(_id, GL.SamplerParameterI.TextureBorderColor, _transparentBorderColorInt);
                    break;
                case BorderColor.IntOpaqueBlack:
                    GL.SamplerParameter(_id, GL.SamplerParameterI.TextureBorderColor, _blackBorderColorInt);
                    break;
                case BorderColor.IntOpaqueWhite:
                    GL.SamplerParameter(_id, GL.SamplerParameterI.TextureBorderColor, _whiteBorderColorInt);
                    break;
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
            GL.SamplerParameter(_id, GL.SamplerParameterI.TextureCompareFunc, (int)samplerDescriptor.CompareOperation);
        }
    }

    public void Dispose()
    {
        GL.DeleteSampler(_id);
    }

    private float ToAnistropy(SampleCount sampleCount)
    {
        return sampleCount switch
        {
            SampleCount.OneSample => 1.0f,
            SampleCount.TwoSamples => 2.0f,
            SampleCount.FourSamples => 4.0f,
            SampleCount.EightSamples => 8.0f,
            SampleCount.SixteenSamples => 16.0f,
            _ => throw new ArgumentOutOfRangeException(nameof(sampleCount), sampleCount, null)
        };
    }
}