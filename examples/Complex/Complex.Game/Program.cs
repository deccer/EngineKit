using Complex.Engine;
using EngineKit;
using Microsoft.Extensions.DependencyInjection;

namespace Complex.Game;

internal static class Program
{
    public static void Main(string[] args)
    {
        using var serviceProvider = ServiceProviderFactory.Create<ComplexGame>();
        var application = serviceProvider.GetRequiredService<IApplication>();
        application.Run();
    }
}
