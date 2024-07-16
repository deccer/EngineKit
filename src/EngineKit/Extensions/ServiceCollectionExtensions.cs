using EngineKit.Graphics;
using EngineKit.Graphics.Assets;
using EngineKit.Graphics.Shaders;
using EngineKit.Input;
using EngineKit.UI;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;

namespace EngineKit.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddEngine(this IServiceCollection services)
    {
        Configuration.Default.StreamProcessingBufferSize = 16384;
        Configuration.Default.PreferContiguousImageBuffers = true;

        services.AddSingleton<IApplicationContext, ApplicationContext>();
        services.AddSingleton<ICapabilities, Capabilities>();
        services.AddSingleton<IMetrics, Metrics>();
        services.AddSingleton<IMessageBus, MessageBus>();

        services.AddSingleton<IFramebufferCache, FramebufferCache>();
        services.AddSingleton<IShaderProgramFactory, ShaderProgramFactory>();
        services.AddSingleton<IShaderParser, ShaderParser>();
        services.AddSingleton<IShaderIncludeHandler, FileShaderIncludeHandler>();
        services.AddSingleton<IShaderIncludeHandler, VirtualFileShaderIncludeHandler>();

        services.AddSingleton<IGraphicsContext, GraphicsContext>();
        services.AddSingleton<IInputProvider, InputProvider>();
        services.AddSingleton<IMeshLoader, SharpGltfMeshLoader>();
        services.AddSingleton<IUIRenderer, UIRenderer>();

        services.AddSingleton<IModelLibrary, ModelLibrary>();
        services.AddSingleton<IMaterialLibrary, MaterialLibrary>();
        services.AddSingleton<ISamplerLibrary, SamplerLibrary>();

        services.AddSingleton<IImageLoader, SixLaborsImageLoader>();
        services.AddSingleton<IKtxImageLoader, KtxImageLoader>();
    }
}
