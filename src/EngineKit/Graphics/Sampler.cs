using System;
using EngineKit.Extensions;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

public class Sampler : IDisposable
{
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

        GL.SamplerParameter(_id, GL.SamplerParameterI.TextureMagFilter, (int)samplerDescriptor.MagFilter.ToGL());
        GL.SamplerParameter(_id, GL.SamplerParameterI.TextureMinFilter, (int)samplerDescriptor.MinFilter.ToGL());
        GL.SamplerParameter(_id, GL.SamplerParameterI.TextureWrapS, (int)samplerDescriptor.AddressModeU.ToGL());
        GL.SamplerParameter(_id, GL.SamplerParameterI.TextureWrapT, (int)samplerDescriptor.AddressModeV.ToGL());
        GL.SamplerParameter(_id, GL.SamplerParameterF.TextureLodBias, samplerDescriptor.LodBias);
        GL.SamplerParameter(_id, GL.SamplerParameterF.TextureMinLod, samplerDescriptor.MinLod);
        GL.SamplerParameter(_id, GL.SamplerParameterF.TextureMaxLod, samplerDescriptor.MaxLod);
    }

    public void Dispose()
    {
        //TODO(deccer) implemenmt
    }
}