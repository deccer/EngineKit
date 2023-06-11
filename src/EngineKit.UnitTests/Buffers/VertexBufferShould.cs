using System.Runtime.InteropServices;
using EngineKit.Graphics;
using EngineKit.Native.OpenGL;
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

#if DEBUG
    [Fact]
#else
    [SkippableFact]
#endif
    public void BeInstantiable()
    {
        // Arrange
#if !DEBUG
        Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
#endif

        var vertexBuffer = new VertexBuffer<VertexPositionNormalUvTangent>("Label");

        // Act
        vertexBuffer.AllocateStorage(100, StorageAllocationFlags.None);

        // Assert
        uint bufferId = vertexBuffer;
        bufferId.Should().BeGreaterThan(0);
        vertexBuffer.Stride.Should().Be(Marshal.SizeOf<VertexPositionNormalUvTangent>());
        vertexBuffer.Count.Should().Be(2);
        vertexBuffer.SizeInBytes.Should().Be(100);
#if DEBUG
        _glfwOpenGLDummyWindow.ErrorMessages.Should().HaveCount(0);
#endif
    }

#if DEBUG
    [Fact]
#else
    [SkippableFact]
#endif
    public void BeAbleToUpdateDynamicBuffer()
    {
        // Arrange
#if !DEBUG
        Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
#endif

        var vertexBuffer = new VertexBuffer<VertexPositionNormalUvTangent>("Label");
        vertexBuffer.AllocateStorage(100, StorageAllocationFlags.Dynamic);

        // Act
        var vertexElements = new VertexPositionNormalUvTangent[]
        {
            new VertexPositionNormalUvTangent(),
            new VertexPositionNormalUvTangent()
        };
        vertexBuffer.Update(vertexElements,  0);

        // Assert
        vertexBuffer.Count.Should().Be(2);
        vertexBuffer.SizeInBytes.Should().Be(100);
        vertexBuffer.Stride.Should().Be(Marshal.SizeOf<VertexPositionNormalUvTangent>());
#if DEBUG
        _glfwOpenGLDummyWindow.ErrorMessages.Should().HaveCount(0);
#endif
    }
}