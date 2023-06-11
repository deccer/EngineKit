using EngineKit.Mathematics;

namespace EngineKit.Graphics;

public sealed class SwapchainRenderDescriptorBuilder
{
    private SwapchainRenderDescriptor _swapchainRenderDescriptor;

    public SwapchainRenderDescriptorBuilder()
    {
        _swapchainRenderDescriptor = new SwapchainRenderDescriptor();
        _swapchainRenderDescriptor.ClearColor = false;
        _swapchainRenderDescriptor.ClearDepth = false;
        _swapchainRenderDescriptor.ClearStencil = false;
        _swapchainRenderDescriptor.ClearColorValue.ColorFloat = new float[4] { 0.0f, 0.0f, 0.0f, 0.0f };
        _swapchainRenderDescriptor.ClearColorValue.ColorSignedInteger = new int[4] { 0, 0, 0, 0 };
        _swapchainRenderDescriptor.ClearColorValue.ColorUnsignedInteger = new uint[4] { 0, 0, 0, 0 };
    }

    public SwapchainRenderDescriptorBuilder WithViewport(int width, int height)
    {
        _swapchainRenderDescriptor.Viewport = new Int4(0, 0, width, height);
        return this;
    }

    public SwapchainRenderDescriptorBuilder WithScissorRectangle(int left, int top, int width, int height)
    {
        _swapchainRenderDescriptor.ScissorRect = new Int4(left, top, width, height);
        return this;
    }

    public SwapchainRenderDescriptorBuilder ClearColor(Color4 clearValue)
    {
        _swapchainRenderDescriptor.ClearColor = true;
        _swapchainRenderDescriptor.ClearColorValue = new ClearColorValue();
        _swapchainRenderDescriptor.ClearColorValue.ColorFloat = new float[4];
        _swapchainRenderDescriptor.ClearColorValue.ColorFloat[0] = clearValue.Red;
        _swapchainRenderDescriptor.ClearColorValue.ColorFloat[1] = clearValue.Green;
        _swapchainRenderDescriptor.ClearColorValue.ColorFloat[2] = clearValue.Blue;
        _swapchainRenderDescriptor.ClearColorValue.ColorFloat[3] = clearValue.Alpha;
        return this;
    }

    public SwapchainRenderDescriptorBuilder ClearDepth(float clearValue = 1.0f)
    {
        _swapchainRenderDescriptor.ClearDepth = true;
        _swapchainRenderDescriptor.ClearDepthValue = clearValue;
        return this;
    }

    public SwapchainRenderDescriptorBuilder ClearStencil(int clearValue = 0)
    {
        _swapchainRenderDescriptor.ClearStencil = true;
        _swapchainRenderDescriptor.ClearStencilValue = clearValue;
        return this;
    }

    public SwapchainRenderDescriptor Build()
    {
        return _swapchainRenderDescriptor;
    }
}