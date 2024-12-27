using EngineKit.Extensions;
using EngineKit.Graphics;
using EngineKit.UI;
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
        services.AddEngineKit();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var graphicsDevice = serviceProvider.GetRequiredService<IGraphicsContext>();
        var uiRenderer = serviceProvider.GetRequiredService<IUIRenderer>();

        // Assert
        graphicsDevice.Should().NotBeNull();
        graphicsDevice.Should().BeAssignableTo<GraphicsContext>();

        uiRenderer.Should().NotBeNull();
        uiRenderer.Should().BeAssignableTo<UIRenderer>();
    }
}
