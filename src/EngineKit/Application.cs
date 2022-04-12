using System;
using System.Data;
using EngineKit.Native.Glfw;
using Serilog;

namespace EngineKit;

public class Application : IApplication
{
    private readonly ILogger _logger;
    private IntPtr _windowHandle;

    protected Application(ILogger logger)
    {
        _logger = logger.ForContext<Application>();
        _windowHandle = IntPtr.Zero;
    }
    
    public void Run()
    {
        if (!Initialize())
        {
            return;
        }

        if (!Load())
        {
            
        }

        while (!Glfw.ShouldWindowClose(_windowHandle))
        {
            Glfw.PollEvents();
            
            Update();
            Render();
            
            Glfw.SwapBuffers(_windowHandle);
        }
    }

    public void Dispose()
    {
        Cleanup();
    }

    protected virtual bool Initialize()
    {
        if (!Glfw.Init())
        {
            _logger.Error("{Category}: Failed to Initialize", "GLFW");
        }
        
        Glfw.WindowHint(Glfw.WindowInitHint.ClientApi, Glfw.ClientApi.OpenGL);
        Glfw.WindowHint(Glfw.WindowInitHint.IsResizeable, true);

        Glfw.WindowHint(Glfw.WindowOpenGLContextHint.VersionMajor, 4);
        Glfw.WindowHint(Glfw.WindowOpenGLContextHint.VersionMinor, 6);
        Glfw.WindowHint(Glfw.WindowOpenGLContextHint.DebugContext, true);
        Glfw.WindowHint(Glfw.WindowOpenGLContextHint.Profile, Glfw.OpenGLProfile.Core);

        var monitorHandle = Glfw.GetPrimaryMonitor();
        var videoMode = Glfw.GetVideoMode(monitorHandle);
        var screenWidth = videoMode.Width;
        var screenHeight = videoMode.Height;
        var windowWidth = (int)(screenWidth * 0.8f);
        var windowHeight = (int)(screenHeight * 0.8f);

        _windowHandle = Glfw.CreateWindow(windowWidth, windowHeight, "Hello", IntPtr.Zero, IntPtr.Zero);
        if (_windowHandle == IntPtr.Zero)
        {
            _logger.Error("{Category}: Failed to create window", "GLFW");
            return false;
        }

        Glfw.SetWindowPos(_windowHandle, screenWidth / 2 - windowWidth / 2, screenHeight / 2 - windowHeight / 2);

        Glfw.SetKeyCallback(_windowHandle, OnKey);
        Glfw.SetCursorPositionCallback(_windowHandle, OnMousePosition);
        Glfw.SetCursorEnterCallback(_windowHandle, OnMouseEnter);
        Glfw.SetMouseButtonCallback(_windowHandle, OnMouseButton);

        Glfw.MakeContextCurrent(_windowHandle);

        return true;
    }

    protected virtual bool Load()
    {
        return true;
    }

    protected virtual void Cleanup()
    {
        Glfw.SetKeyCallback(_windowHandle, null);
        Glfw.SetCursorEnterCallback(_windowHandle, null);
        Glfw.SetCursorPositionCallback(_windowHandle, null);
        Glfw.SetMouseButtonCallback(_windowHandle, null);
        Glfw.DestroyWindow(_windowHandle);
        Glfw.Terminate();
    }

    protected virtual void Update()
    {
    }

    protected virtual void Render()
    {
    }
    
    private void OnKey(
        IntPtr windowHandle,
        Glfw.Key key,
        int scanCode,
        Glfw.KeyAction action,
        Glfw.KeyModifiers modifiers)
    {
        _logger.Debug("key: {Key} scancode: {ScanCode} action: {Action} modifiers: {Modifiers}", key, scanCode, action, modifiers);
    }

    private void OnMousePosition(
        IntPtr windowHandle,
        double x,
        double y)
    {
        _logger.Debug("x: {X} y: {Y}", x, y);
    }

    private void OnMouseEnter(
        IntPtr windowHandle,
        Glfw.CursorEnterMode cursorEnterMode)
    {
        _logger.Debug("mode: {CursorEnterMode}", cursorEnterMode);
    }

    private void OnMouseButton(
        IntPtr windowHandle,
        Glfw.MouseButton mouseButton,
        Glfw.KeyAction action,
        Glfw.KeyModifiers modifiers)
    {
        _logger.Debug("button: {MouseButton} action: {Action} modifiers: {Modifiers}", mouseButton, action, modifiers);
    }
}