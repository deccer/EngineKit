using System.Runtime.InteropServices;
using EngineKit.Graphics;
using EngineKit.UnitTests.TestInfrastructure;
using FluentAssertions;
using OpenTK.Mathematics;
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

#if DEBUG
    [Fact]
#else
    [SkippableFact]
#endif
    public void BeInstantiable()
    {
        // Arrange & Act
#if !DEBUG
        Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
#endif
        var uniformBuffer = new UniformBuffer<GpuGlobalMatrices>("Label");

        // Assert
        uint bufferId = uniformBuffer;
        bufferId.Should().BeGreaterThan(0);
        uniformBuffer.Stride.Should().Be(Marshal.SizeOf<GpuGlobalMatrices>());
        uniformBuffer.Count.Should().Be(0);
        uniformBuffer.SizeInBytes.Should().Be(0);
    }

#if DEBUG
    [Fact]
#else
    [SkippableFact]
#endif
    public void BeAbleToUpdateDynamicBufferWhenInitializedWithZeroSize()
    {
        // Arrange
#if !DEBUG
        Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
#endif

        var uniformBuffer = new UniformBuffer<GpuGlobalMatrices>("Label");
        uniformBuffer.AllocateStorage(Marshal.SizeOf<GpuGlobalMatrices>(), StorageAllocationFlags.None);

        // Act
        var globalMatrices = new GpuGlobalMatrices
        {
            WorldToCameraMatrix = Matrix4.Identity
        };
        uniformBuffer.Update(globalMatrices,  0);

        // Assert
        uniformBuffer.Count.Should().Be(1);
        uniformBuffer.Stride.Should().Be(Marshal.SizeOf<GpuGlobalMatrices>());
        uniformBuffer.SizeInBytes.Should().Be(uniformBuffer.Stride);
    }

#if DEBUG
    [Theory]
#else
    [SkippableTheory]
#endif
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
#if !DEBUG
        Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
#endif

        var uniformBuffer = new UniformBuffer<GpuGlobalMatrices>("Label");
        uniformBuffer.AllocateStorage(initialElementCount * Marshal.SizeOf<GpuGlobalMatrices>(), StorageAllocationFlags.Dynamic);

        // Act
        var globalMatrices = new GpuGlobalMatrices
        {
            WorldToCameraMatrix = Matrix4.Identity
        };
        uniformBuffer.Update(globalMatrices,  0);

        // Assert
        uniformBuffer.Count.Should().Be(1);
        uniformBuffer.SizeInBytes.Should().Be(initialElementCount * uniformBuffer.Stride);
    }
}