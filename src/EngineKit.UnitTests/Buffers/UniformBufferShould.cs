using System.Runtime.InteropServices;
using EngineKit.Graphics;
using EngineKit.UnitTests.TestInfrastructure;
using FluentAssertions;
using EngineKit.Mathematics;
using Xunit;

namespace EngineKit.UnitTests.Buffers;

[Collection("Serial-Test-Collection")]
public class UniformBufferShould : IClassFixture<GlfwOpenGLDummyWindow>
{
    // ReSharper disable once NotAccessedField.Local
    private readonly GlfwOpenGLDummyWindow _glfwOpenGLDummyWindow;

    public UniformBufferShould(GlfwOpenGLDummyWindow glfwOpenGLDummyWindow)
    {
        _glfwOpenGLDummyWindow = glfwOpenGLDummyWindow;
    }

    [Fact]
    public void BeInstantiable()
    {
        // Arrange & Act
        var uniformBuffer = new Buffer("Label", 0u);

        // Assert
        uint bufferId = uniformBuffer;
        bufferId.Should().BeGreaterThan(0);
        uniformBuffer.SizeInBytes.Should().Be(4);
    }

    [Theory]
    [InlineData(512)]
    [InlineData(256)]
    [InlineData(128)]
    [InlineData(64)]
    [InlineData(32)]
    [InlineData(16)]
    [InlineData(8)]
    [InlineData(4)]
    [InlineData(2)]
    [InlineData(1)]
    public unsafe void BeAbleToUpdateDynamicBuffer(uint initialElementCount)
    {
        // Arrange
        var uniformBuffer = new TypedBuffer<GpuMaterial>("Label", initialElementCount);

        // Act
        var globalMatrices = new GpuMaterial
        {
            BaseColorFactor = Colors.Red.ToVector4()
        };
        uniformBuffer.UpdateElement(ref globalMatrices,  0);

        // Assert
        uniformBuffer.SizeInBytes.Should().Be((nuint)(initialElementCount * sizeof(GpuMaterial)));
    }
}