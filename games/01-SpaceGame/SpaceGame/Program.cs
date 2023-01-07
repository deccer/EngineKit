using EngineKit;
using EngineKit.Extensions;
using EngineKit.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTK.Mathematics;
using Serilog;
using SpaceGame.Game;
using SpaceGame.Game.Physics;

namespace SpaceGame;

public static class Program
{
    public static void Main()
    {
        using var serviceProvider = CreateServiceProvider();
        using var application = serviceProvider.GetRequiredService<IApplication>();

        application.Run();
    }

    private static ServiceProvider CreateServiceProvider()
    {
        SixLabors.ImageSharp.Configuration.Default.PreferContiguousImageBuffers = true;

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false)
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddSingleton(Log.Logger);
        services.AddSingleton<IMessageBus, MessageBus>();
        services.Configure<WindowSettings>(configuration.GetSection(nameof(WindowSettings)));
        services.Configure<ContextSettings>(configuration.GetSection(nameof(ContextSettings)));
        services.AddEngine();
        services.AddSingleton<ICamera>(provider => new Camera(
            provider.GetRequiredService<IApplicationContext>(),
            provider.GetRequiredService<IInputProvider>(), new Vector3(0, 0, 10), Vector3.UnitY));
        services.AddSingleton<IApplication, ConflictGameApplication>();
        services.AddSingleton<IRendererContext, RendererContext>();
        services.AddSingleton<IRenderer, DeferredRenderer>();
        services.AddSingleton<IModelLoader, ModelLoader>();
        services.AddSingleton<IModelLibrary, ModelLibrary>();
        services.AddSingleton<IPhysicsWorld, JoltPhysicsWorld>();

        return services.BuildServiceProvider();
    }
}