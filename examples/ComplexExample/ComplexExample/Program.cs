﻿using EngineKit;
using EngineKit.Extensions;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Mathematics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ComplexExample;

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
        services.AddSingleton<IApplication, ComplexExampleApplication>();
        services.AddSingleton<IScene, Scene>();
        services.AddSingleton<IRenderer, Renderer>();
        services.AddSingleton<ICamera>(provider => new Camera(
            provider.GetRequiredService<IApplicationContext>(),
            provider.GetRequiredService<IInputProvider>(), 
            new Vector3(0, 0, 10), 
            Vector3.UnitY));
        
        return services.BuildServiceProvider();
    }
}