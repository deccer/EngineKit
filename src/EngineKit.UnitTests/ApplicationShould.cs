using EngineKit.Input;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Serilog;
using Xunit;

namespace EngineKit.UnitTests;

public class ApplicationShould
{
    private readonly Application _sut;

    public ApplicationShould()
    {
        var logger = Substitute.For<ILogger>();
        var windowSettings = Substitute.For<IOptions<WindowSettings>>();
        var contextSettings = Substitute.For<IOptions<ContextSettings>>();
        var applicationContext = Substitute.For<IApplicationContext>();
        var limits = Substitute.For<ILimits>();
        var metrics = Substitute.For<IMetrics>();
        var inputProvider = Substitute.For<IInputProvider>();

        _sut = Substitute.ForPartsOf<Application>(logger,
            windowSettings,
            contextSettings,
            applicationContext,
            limits,
            metrics,
            inputProvider);
    }

    [Fact]
    public void BeInstantiable()
    {
        _sut.Should().NotBeNull();
        _sut.Should().BeAssignableTo<IApplication>();
    }
}