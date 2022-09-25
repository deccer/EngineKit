using EngineKit.Graphics;
using EngineKit.Graphics.MeshLoaders;
using EngineKit.Input;
using EngineKit.UI;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;

namespace EngineKit.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEngine(this IServiceCollection services)
    {
        Configuration.Default.StreamProcessingBufferSize = 16384;
        Configuration.Default.PreferContiguousImageBuffers = true;
        services.AddSingleton<IApplicationContext, ApplicationContext>();
        services.AddSingleton<IMetrics, Metrics>();
        services.AddSingleton<IInputProvider, InputProvider>();

        services.AddSingleton<IFramebufferFactory, FramebufferFactory>();
        services.AddSingleton<IGraphicsContext, GraphicsContext>();
        services.AddSingleton<IGraphicsPipelineDescriptorBuilder, GraphicsPipelineDescriptorBuilder>();
        services.AddSingleton<ITextureLoader, TextureLoader>();
        services.AddSingleton<ITextureLibrary, TextureLibrary>();
        services.AddSingleton<IMaterialLibrary, MaterialLibrary>();
        services.AddSingleton<IImageLibrary, ImageLibrary>();
        services.AddSingleton<IApplicationContext, ApplicationContext>();
        services.AddSingleton<IInputProvider, InputProvider>();
        services.AddSingleton<IMeshLoader, SharpGltfMeshLoader>();
        services.AddSingleton<IUIRenderer, UIRenderer>();

        return services;
    }
}