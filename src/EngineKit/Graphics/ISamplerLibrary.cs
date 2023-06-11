using System;

namespace EngineKit.Graphics;

public interface ISamplerLibrary : IDisposable
{
    ISampler GetSampler(SamplerInformation samplerInformation);
    
    void AddSamplerIfNotExists(SamplerInformation? samplerInformation);
}