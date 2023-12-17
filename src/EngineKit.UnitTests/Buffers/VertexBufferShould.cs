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
        // Arrange
        var vertexBuffer = new Buffer<VertexPositionNormalUvTangent>(BufferTarget.VertexBuffer, "Label");

        // Act
        vertexBuffer.AllocateStorage(100, StorageAllocationFlags.None);

        // Assert
        uint bufferId = vertexBuffer;
        bufferId.Should().BeGreaterThan(0);
        vertexBuffer.Stride.Should().Be(Marshal.SizeOf<VertexPositionNormalUvTangent>());
        vertexBuffer.Count.Should().Be(2);
        vertexBuffer.SizeInBytes.Should().Be(100);
        _glfwOpenGLDummyWindow.ErrorMessages.Should().HaveCount(0);
    }

    [Fact]
    public void BeAbleToUpdateDynamicBuffer()
    {
        // Arrange
        var vertexBuffer = new Buffer<VertexPositionNormalUvTangent>(BufferTarget.VertexBuffer, "Label");
        vertexBuffer.AllocateStorage(100, StorageAllocationFlags.Dynamic);

        // Act
        var vertexElements = new VertexPositionNormalUvTangent[]
        {
            new VertexPositionNormalUvTangent(),
            new VertexPositionNormalUvTangent()
        };
        vertexBuffer.Update(ref vertexElements,  0);

        // Assert
        vertexBuffer.Count.Should().Be(2);
        vertexBuffer.SizeInBytes.Should().Be(100);
        vertexBuffer.Stride.Should().Be(Marshal.SizeOf<VertexPositionNormalUvTangent>());
        _glfwOpenGLDummyWindow.ErrorMessages.Should().HaveCount(0);
    }
}