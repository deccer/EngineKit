using FluentAssertions;
using NSubstitute;
using Serilog;
using Xunit;

namespace EngineKit.UnitTests;

public class ApplicationShould
{
    private readonly ILogger _logger;
    private Application _sut;

    public ApplicationShould()
    {
        _logger = Substitute.For<ILogger>();

        _sut = new Application(_logger);
    }

    [Fact]
    public void BeInstantiable()
    {
        _sut.Should().NotBeNull();
        _sut.Should().BeAssignableTo<IApplication>();
    }
}