using System.Numerics;
using Complex.Windows;
using EngineKit;
using EngineKit.Extensions;
using EngineKit.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Complex;

using Complex.Engine;
using Complex.Engine.Ecs;
using Complex.Engine.Ecs.Systems;
using Complex.Engine.Physics;
using EngineKit.Graphics;
using Microsoft.Extensions.DependencyInjection.Extensions;

internal static class Program
{
    public static void Main(string[] args)
    {
        using var serviceProvider = CreateServiceProvider();

        var application = serviceProvider.GetRequiredService<IApplication>();
        application.Run();
    }

    private static ServiceProvider CreateServiceProvider()
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

        services.AddSingleton<IApplication, ComplexApplication>();
        services.AddSingleton<ICamera>(provider => new Camera(
            provider.GetRequiredService<IApplicationContext>(),
            provider.GetRequiredService<IInputProvider>(), new Vector3(0, 2, 10), Vector3.UnitY));

        services.AddEngineKit();

        services.AddSingleton<IRenderer2, Renderer>();

        services.AddSingleton<AssetWindow>();
        services.AddSingleton<SceneHierarchyWindow>();
        services.AddSingleton<SceneViewWindow>();
        services.AddSingleton<PropertyWindow>();

        services.AddSingleton<Editor>();
        services.AddSingleton<Game>();
        services.AddSingleton<IScene, Scene>();
        services.AddSingleton<IEntityRegistry, EntityRegistry>();
        services.AddSingleton<ISystemsUpdater, SystemsUpdater>();
        services.AddSingleton<IUpdateCameraSystem, UpdateCameraSystem>();
        services.AddSingleton<ITransformSystem, TransformSystem>();
        services.AddSingleton<IPreRenderSystem, PreRenderSystem>();
        services.AddSingleton<IMaterialLibrary, MaterialLibrary>();
        services.AddSingleton<ISamplerLibrary, SamplerLibrary>();
        services.AddSingleton<IPhysicsWorld, PhysicsWorld>();

        return services.BuildServiceProvider();
    }
}
