using EngineKit;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace HelloWindow;

internal class Program
{
    public static void Main(string[] args)
    {
        using var serviceProvider = CreateServiceProvider();

        var application = serviceProvider.GetRequiredService<IApplication>();
        application.Run();
    }

    private static ServiceProvider CreateServiceProvider()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        var services = new ServiceCollection();
        services.AddSingleton(Log.Logger);
        services.AddSingleton<IApplication, HelloWindowApplication>();
        return services.BuildServiceProvider();
    }
}