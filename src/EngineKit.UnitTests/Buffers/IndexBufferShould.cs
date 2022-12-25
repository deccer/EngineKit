using System.Runtime.InteropServices;
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
        var indexBuffer = new IndexBuffer<uint>("Label");
        indexBuffer.AllocateStorage(100, StorageAllocationFlags.None);

        // Assert
        uint bufferId = indexBuffer;
        bufferId.Should().BeGreaterThan(0);
        indexBuffer.Stride.Should().Be(Marshal.SizeOf<uint>());
        indexBuffer.Count.Should().Be(0);
        indexBuffer.SizeInBytes.Should().Be(100);
    }

    [Fact]
    public void BeAbleToUpdateDynamicBuffer()
    {
        // Arrange
        var indexBuffer = new IndexBuffer<uint>("Label");
        indexBuffer.AllocateStorage(100, StorageAllocationFlags.Dynamic);

        // Act
        var indices = new uint[]
        {
            100,
            200
        };
        indexBuffer.Update(indices,  0);

        // Assert
        indexBuffer.Count.Should().Be(2);
        indexBuffer.SizeInBytes.Should().Be(100);
    }
}