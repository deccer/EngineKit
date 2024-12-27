using System.Numerics;
using Complex.Engine.Ecs;
using Complex.Engine.Ecs.Systems;
using Complex.Engine.Physics;
using EngineKit;
using EngineKit.Extensions;
using EngineKit.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Complex.Engine;

public static class ServiceProviderFactory
{
    public static ServiceProvider Create<TGame>() where TGame : class, IGame
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false)
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddSingleton(Log.Logger);
        services.Configure<WindowSettings>(configuration.GetSection(nameof(WindowSettings)));
        services.Configure<ContextSettings>(configuration.GetSection(nameof(ContextSettings)));

        services.AddSingleton<TGame>();
        services.AddSingleton<IApplication, GameApplication<TGame>>();
        services.AddSingleton<ICamera>(provider => new Camera(
            provider.GetRequiredService<IApplicationContext>(),
            provider.GetRequiredService<IInputProvider>(), new Vector3(0, 2, 10), Vector3.UnitY));

        services.AddEngineKit();
        services.AddSingleton<IPhysicsWorld, PhysicsWorld>();
        services.AddSingleton<IEntityRegistry, EntityRegistry>();
        services.AddSingleton<IRenderer2, Renderer>();
        services.AddSingleton<IUpdateCameraSystem, UpdateCameraSystem>();
        services.AddSingleton<IPreRenderSystem, PreRenderSystem>();
        services.AddSingleton<ITransformSystem, TransformSystem>();
        services.AddSingleton<ISystemsUpdater, SystemsUpdater>();
        services.AddSingleton<IScene, Scene>();

        return services.BuildServiceProvider();
    }
}
