using System.Runtime.InteropServices;
using EngineKit.Graphics;
using EngineKit.UnitTests.TestInfrastructure;
using FluentAssertions;
using Xunit;

namespace EngineKit.UnitTests.Buffers;

[Collection("Serial-Test-Collection")]
public class VertexBufferShould : IClassFixture<GlfwOpenGLDummyWindow>
{
    // ReSharper disable once NotAccessedField.Local
    private readonly GlfwOpenGLDummyWindow _glfwOpenGLDummyWindow;

    public VertexBufferShould(GlfwOpenGLDummyWindow glfwOpenGLDummyWindow)
    {
        _glfwOpenGLDummyWindow = glfwOpenGLDummyWindow;
    }

    [Fact]
    public void BeInstantiable()
    {
        // Arrange & Act
        var vertexBuffer = new Buffer("Label", 100);

        // Assert
        uint bufferId = vertexBuffer;
        bufferId.Should().BeGreaterThan(0);
        vertexBuffer.SizeInBytes.Should().Be(100);
        _glfwOpenGLDummyWindow.ErrorMessages.Should().HaveCount(0);
    }

    [Fact]
    public void BeAbleToUpdateDynamicBuffer()
    {
        // Arrange
        var vertexBuffer = new TypedBuffer<VertexPositionNormalUvTangent>("Label", 100);

        // Act
        var vertexElements = new VertexPositionNormalUvTangent[]
        {
            new VertexPositionNormalUvTangent(),
            new VertexPositionNormalUvTangent()
        };
        vertexBuffer.UpdateElements(ref vertexElements,  0);

        // Assert
        vertexBuffer.SizeInBytes.Should().Be(100 * (uint)Marshal.SizeOf<VertexPositionNormalUvTangent>());
        _glfwOpenGLDummyWindow.ErrorMessages.Should().HaveCount(0);
    }
}