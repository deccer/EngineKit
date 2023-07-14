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
        var uniformBuffer = new UniformBuffer<GpuMaterial>("Label");

        // Assert
        uint bufferId = uniformBuffer;
        bufferId.Should().BeGreaterThan(0);
        uniformBuffer.Stride.Should().Be(Marshal.SizeOf<GpuMaterial>());
        uniformBuffer.Count.Should().Be(0);
        uniformBuffer.SizeInBytes.Should().Be(0);
    }

    [Fact]
    public void BeAbleToUpdateDynamicBufferWhenInitializedWithZeroSize()
    {
        // Arrange
        var uniformBuffer = new UniformBuffer<GpuMaterial>("Label");
        uniformBuffer.AllocateStorage(Marshal.SizeOf<GpuMaterial>(), StorageAllocationFlags.None);

        // Act
        var globalMatrices = new GpuMaterial
        {
            BaseColorFactor = Color.Red.ToVector4()
        };
        uniformBuffer.Update(globalMatrices,  0);

        // Assert
        uniformBuffer.Count.Should().Be(1);
        uniformBuffer.Stride.Should().Be(Marshal.SizeOf<GpuMaterial>());
        uniformBuffer.SizeInBytes.Should().Be(uniformBuffer.Stride);
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
    public void BeAbleToUpdateDynamicBuffer(int initialElementCount)
    {
        // Arrange
        var uniformBuffer = new UniformBuffer<GpuMaterial>("Label");
        uniformBuffer.AllocateStorage(initialElementCount * Marshal.SizeOf<GpuMaterial>(), StorageAllocationFlags.Dynamic);

        // Act
        var globalMatrices = new GpuMaterial
        {
            BaseColorFactor = Color.Red.ToVector4()
        };
        uniformBuffer.Update(globalMatrices,  0);

        // Assert
        uniformBuffer.Count.Should().Be(initialElementCount);
        uniformBuffer.SizeInBytes.Should().Be(initialElementCount * uniformBuffer.Stride);
    }
}