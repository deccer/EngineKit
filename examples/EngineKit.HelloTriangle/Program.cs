using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace EngineKit.HelloWindow;

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
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false)
            .Build();
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddSingleton(Log.Logger);
        services.AddSingleton<IApplication, HelloWindowApplication>();
        return services.BuildServiceProvider();
    }
}
