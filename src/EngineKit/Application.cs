using System;
using EngineKit.Native.Glfw;
using Serilog;

namespace EngineKit;

public class Application : IApplication
{
    private readonly ILogger _logger;
    private IntPtr _windowHandle;

    private Glfw.FramebufferSizeCallback? _framebufferSizeCallback;

    public Application(ILogger logger)
    {
        _logger = logger.ForContext<Application>();
    }

    public void Dispose()
    {
        Glfw.Terminate();
    }

    public void Run()
    {
        if (!Initialize())
        {
            return;
        }

        _logger.Debug("App: Initialized");

        if (!Load())
        {
            return;
        }

        _logger.Debug("App: Loaded");

        while (!Glfw.ShouldWindowClose(_windowHandle))
        {
            Glfw.PollEvents();

            Update();
            Render();

            Glfw.SwapBuffers(_windowHandle);
        }

        _logger.Debug("App: Unloading");

        Unload();

        _logger.Debug("App: Unloaded");
    }

    protected virtual bool Initialize()
    {
        if (!Glfw.Init())
        {
            _logger.Error("Glfw: Unable to initialize");
            return false;
        }

        Glfw.WindowHint(Glfw.WindowInitHint.ClientApi, Glfw.ClientApi.OpenGL);
        Glfw.WindowHint(Glfw.WindowInitHint.IsResizeable, true);
        Glfw.WindowHint(Glfw.WindowInitHint.ScaleToMonitor, false);
        Glfw.WindowHint(Glfw.WindowOpenGLContextHint.Profile, Glfw.OpenGLProfile.Core);
        Glfw.WindowHint(Glfw.WindowOpenGLContextHint.DebugContext, true);
        Glfw.WindowHint(Glfw.WindowOpenGLContextHint.VersionMajor, 4);
        Glfw.WindowHint(Glfw.WindowOpenGLContextHint.VersionMinor, 6);

        _windowHandle = Glfw.CreateWindow(1920, 1080, "EngineKit", IntPtr.Zero, IntPtr.Zero);
        if (_windowHandle == IntPtr.Zero)
        {
            _logger.Error("Glfw: Unable to create window");
            return false;
        }

        BindCallbacks();

        return true;
    }

    protected virtual bool Load()
    {
        return true;
    }

    protected virtual void Render()
    {
    }

    protected virtual void Unload()
    {
        UnbindCallbacks();
    }

    protected virtual void Update()
    {
    }

    private void BindCallbacks()
    {
        _framebufferSizeCallback = OnFramebufferSize;
        Glfw.SetFramebufferSizeCallback(_windowHandle, _framebufferSizeCallback);
    }

    private void UnbindCallbacks()
    {
        Glfw.SetFramebufferSizeCallback(_windowHandle, null);
    }

    private void OnFramebufferSize(IntPtr windowHandle, int width, int height)
    {

    }
}