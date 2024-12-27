using EngineKit.Core;
using EngineKit.Mathematics;

namespace EngineKit.Graphics;

public interface ISwapchainDescriptorBuilder
{
    ISwapchainDescriptorBuilder EnableSrgb();

    ISwapchainDescriptorBuilder WithViewport(int width,
        int height);

    ISwapchainDescriptorBuilder WithScissorRectangle(int left,
        int top,
        int width,
        int height);

    ISwapchainDescriptorBuilder ClearColor(Color4 clearValue);

    ISwapchainDescriptorBuilder ClearDepth(float clearValue);

    ISwapchainDescriptorBuilder ClearStencil(int clearValue);

    ISwapchainDescriptorBuilder WithFramebufferSizeAsViewport();

    ISwapchainDescriptorBuilder WithScaledFramebufferSizeAsViewport();

    ISwapchainDescriptorBuilder WithWindowSizeAsViewport();

    SwapchainDescriptor Build(Label label);
}
