using EngineKit.Extensions;
using EngineKit.Graphics;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace EngineKit.UnitTests;

public class DependenciesShould
{
    [Xunit.Fact]
    public void Resolve()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false)
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
        services.AddSingleton(configuration);
        services.AddSingleton(Log.Logger);
        services.AddEngine();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var graphicsDevice = serviceProvider.GetRequiredService<IGraphicsContext>();

        // Assert
        graphicsDevice.Should().NotBeNull();
        graphicsDevice.Should().BeAssignableTo<GraphicsContext>();
    }
}