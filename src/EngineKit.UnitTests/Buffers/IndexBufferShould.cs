using EngineKit.Graphics;
using EngineKit.UnitTests.TestInfrastructure;
using FluentAssertions;
using Xunit;

namespace EngineKit.UnitTests.Buffers;

[Collection("Serial-Test-Collection")]
public class IndexBufferShould : IClassFixture<GlfwOpenGLDummyWindow>
{
    // ReSharper disable once NotAccessedField.Local
    private readonly GlfwOpenGLDummyWindow _glfwOpenGLDummyWindow;

    public IndexBufferShould(GlfwOpenGLDummyWindow glfwOpenGLDummyWindow)
    {
        _glfwOpenGLDummyWindow = glfwOpenGLDummyWindow;
    }

    [Fact]
    public void BeInstantiable()
    {
        // Arrange & Act
        var indexBuffer = new Buffer("Label", 100);

        // Assert
        uint bufferId = indexBuffer;
        bufferId.Should().BeGreaterThan(0);
        indexBuffer.SizeInBytes.Should().Be(100);
    }

    [Fact]
    public void BeAbleToUpdateDynamicBuffer()
    {
        // Arrange
        var indexBuffer = new TypedBuffer<uint>("Label", 100);

        // Act
        var indices = new uint[]
        {
            100,
            200
        };
        indexBuffer.UpdateElements(in indices,  0);

        // Assert
        indexBuffer.SizeInBytes.Should().Be(400);
    }
}