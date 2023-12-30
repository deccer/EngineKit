﻿using System.Numerics;
using Complex.Ecs;
using Complex.Ecs.Systems;
using Complex.Windows;
using EngineKit;
using EngineKit.Extensions;
using EngineKit.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Complex;

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
        services.AddEngine();
        services.AddSingleton<IApplication, ComplexApplication>();
        services.AddSingleton<ICamera>(provider => new Camera(provider.GetRequiredService<IApplicationContext>(),
            provider.GetRequiredService<IInputProvider>(), new Vector3(0, 2, 10), Vector3.UnitY));
        services.AddSingleton<IPhysicsWorld, PhysicsWorld>();
        services.AddSingleton<IEntityWorld, EntityWorld>();
        services.AddSingleton<IRenderer, Renderer>();
        services.AddSingleton<IUpdateCameraSystem, UpdateCameraSystem>();
        services.AddSingleton<IPreRenderSystem, PreRenderSystem>();
        services.AddSingleton<ITransformSystem, TransformSystem>();
        services.AddSingleton<ISystemsUpdater, SystemsUpdater>();

        services.AddSingleton<AssetWindow>();
        services.AddSingleton<SceneWindow>();
        services.AddSingleton<PropertyWindow>();
        
        services.AddSingleton<IAssetLoader, AssetLoader>();
        services.AddSingleton<IScene, Scene>();
        return services.BuildServiceProvider();
    }
}