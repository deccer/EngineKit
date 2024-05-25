using EngineKit.Mathematics;

namespace EngineKit.Graphics;

internal sealed class SwapchainDescriptorBuilder : ISwapchainDescriptorBuilder
{
    private readonly IApplicationContext _applicationContext;
    private SwapchainDescriptor _swapchainDescriptor;

    public SwapchainDescriptorBuilder(IApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
        Reset();
    }

    public ISwapchainDescriptorBuilder Reset()
    {
        _swapchainDescriptor = new SwapchainDescriptor
        {
            ClearColor = false,
            ClearDepth = false,
            ClearStencil = false
        };
        _swapchainDescriptor.ClearColorValue.ColorFloat = [0.0f, 0.0f, 0.0f, 0.0f];
        _swapchainDescriptor.ClearColorValue.ColorSignedInteger = [0, 0, 0, 0];
        _swapchainDescriptor.ClearColorValue.ColorUnsignedInteger = [0, 0, 0, 0];
        return this;
    }

    public ISwapchainDescriptorBuilder EnableSrgb()
    {
        _swapchainDescriptor.EnableSrgb = true;
        return this;
    }

    public ISwapchainDescriptorBuilder WithViewport(int width, int height)
    {
        _swapchainDescriptor.Viewport = new Viewport(0, 0, width, height);
        return this;
    }

    public ISwapchainDescriptorBuilder WithFramebufferSizeAsViewport()
    {
        _swapchainDescriptor.Viewport = new Viewport(
            0, 
            0, 
            _applicationContext.WindowFramebufferSize.X,
            _applicationContext.WindowFramebufferSize.Y);
        return this;
    }

    public ISwapchainDescriptorBuilder WithScaledFramebufferSizeAsViewport()
    {
        _swapchainDescriptor.Viewport = new Viewport(
            0, 
            0, 
            _applicationContext.WindowScaledFramebufferSize.X,
            _applicationContext.WindowScaledFramebufferSize.Y);
        return this;
    }
    
    public ISwapchainDescriptorBuilder WithWindowSizeAsViewport()
    {
        _swapchainDescriptor.Viewport = new Viewport(
            0, 
            0, 
            _applicationContext.WindowSize.X,
            _applicationContext.WindowSize.Y);
        return this;
    }

    public ISwapchainDescriptorBuilder WithScissorRectangle(int left, int top, int width, int height)
    {
        _swapchainDescriptor.ScissorRect = new Viewport(left, top, width, height);
        return this;
    }

    public ISwapchainDescriptorBuilder ClearColor(Color4 clearValue)
    {
        _swapchainDescriptor.ClearColor = true;
        _swapchainDescriptor.ClearColorValue = new ClearColorValue();
        _swapchainDescriptor.ClearColorValue.ColorFloat = new float[4];
        _swapchainDescriptor.ClearColorValue.ColorFloat[0] = clearValue.R;
        _swapchainDescriptor.ClearColorValue.ColorFloat[1] = clearValue.G;
        _swapchainDescriptor.ClearColorValue.ColorFloat[2] = clearValue.B;
        _swapchainDescriptor.ClearColorValue.ColorFloat[3] = clearValue.A;
        return this;
    }

    public ISwapchainDescriptorBuilder ClearDepth(float clearValue)
    {
        _swapchainDescriptor.ClearDepth = true;
        _swapchainDescriptor.ClearDepthValue = clearValue;
        return this;
    }

    public ISwapchainDescriptorBuilder ClearStencil(int clearValue)
    {
        _swapchainDescriptor.ClearStencil = true;
        _swapchainDescriptor.ClearStencilValue = clearValue;
        return this;
    }

    public SwapchainDescriptor Build(Label label)
    {
        _swapchainDescriptor.Label = label;
        return _swapchainDescriptor;
    }
}