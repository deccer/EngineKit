using EngineKit.Graphics;
using Microsoft.Extensions.DependencyInjection;

namespace EngineKit.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEngine(this IServiceCollection services)
    {
        services.AddSingleton<IApplicationContext, ApplicationContext>();
        services.AddSingleton<IMetrics, Metrics>();
        services.AddSingleton<IInputProvider, InputProvider>();

        services.AddSingleton<IFramebufferFactory, FramebufferFactory>();
        services.AddSingleton<IGraphicsContext, GraphicsContext>();
        services.AddSingleton<IGraphicsPipelineDescriptorBuilder, GraphicsPipelineDescriptorBuilder>();
        services.AddSingleton<IMeshFactory, MeshFactory>();
        services.AddSingleton<ITextureLoader, TextureLoader>();
        services.AddSingleton<ITextureLibrary, TextureLibrary>();
        services.AddSingleton<IMaterialLibrary, MaterialLibrary>();
        services.AddSingleton<IImageLibrary, ImageLibrary>();
        services.AddSingleton<IApplicationContext, ApplicationContext>();
        services.AddSingleton<IInputProvider, InputProvider>();

        return services;
    }
}