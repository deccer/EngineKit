using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using EngineKit.Mathematics;
using EngineKit.Native.Glfw;
using EngineKit.Native.OpenGL;
using Serilog;

namespace EngineKit;

public class Application : IApplication
{
    private readonly IApplicationContext _applicationContext;
    private readonly IMetrics _metrics;
    private readonly IInputProvider _inputProvider;
    private readonly ILogger _logger;
    private IntPtr _windowHandle;

    private GL.GLDebugProc? _debugProcCallback;
    private Glfw.FramebufferSizeCallback? _framebufferSizeCallback;
    private Glfw.KeyCallback? _keyCallback;
    private Glfw.MouseButtonCallback? _mouseButtonCallback;
    private Glfw.CursorEnterCallback? _cursorEnterLeaveCallback;
    private Glfw.CursorPositionCallback? _cursorPositionCallback;
    private Glfw.WindowSizeCallback? _windowSizeCallback;

    private readonly bool _showUpdatesPerSecond = true;
    private readonly int _showUpdatesPerSecondRate = 1000;

    public Application(
        ILogger logger,
        IApplicationContext applicationContext,
        IMetrics metrics,
        IInputProvider inputProvider)
    {
        _logger = logger.ForContext<Application>();
        _applicationContext = applicationContext;
        _metrics = metrics;
        _inputProvider = inputProvider;
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

        var stopwatch = Stopwatch.StartNew();
        var accumulator = 0f;
        var currentTime = stopwatch.ElapsedMilliseconds;
        var lastTime = currentTime;
        var nextUpdate = lastTime + _showUpdatesPerSecondRate;
        _metrics.UpdateRate = 1.0f / 60.0f;

        while (!Glfw.ShouldWindowClose(_windowHandle))
        {
            Glfw.PollEvents();
            _inputProvider.MouseState.Center();

            currentTime = stopwatch.ElapsedMilliseconds;
            var deltaTime = currentTime - lastTime;
            var lastRenderTimeInSeconds = deltaTime / (float)_showUpdatesPerSecondRate;
            accumulator += lastRenderTimeInSeconds;
            lastTime = currentTime;

            while (accumulator >= _metrics.UpdateRate)
            {
                FixedUpdate();
                if (Glfw.ShouldWindowClose(_windowHandle))
                {
                    break;
                }

                accumulator -= _metrics.UpdateRate;
                _metrics.UpdatesPerSecond++;
            }

            Update();
            Render();

            var swapBufferStart = Stopwatch.StartNew();
            Glfw.SwapBuffers(_windowHandle);
            _metrics.SwapBufferDuration = swapBufferStart.Elapsed.TotalMilliseconds;
            _metrics.FrameCounter++;
            _metrics.FramesPerSecond++;

            if (_showUpdatesPerSecond && stopwatch.ElapsedMilliseconds >= nextUpdate)
            {
                _logger.Debug("FPS: {@FramesPerSecond} UPS: {@UpdatesPerSecond} UR: {@UpdateRate} SD: {SwapBufferDuration:F3}ms",
                    _metrics.FramesPerSecond,
                    _metrics.UpdatesPerSecond,
                    _metrics.UpdateRate,
                    _metrics.SwapBufferDuration);
                _metrics.UpdatesPerSecond = 0;
                _metrics.FramesPerSecond = 0;
                nextUpdate = stopwatch.ElapsedMilliseconds + _showUpdatesPerSecondRate;
            }
        }

        _logger.Debug("App: Unloading");

        Unload();

        _logger.Debug("App: Unloaded");
    }

    protected virtual void FixedUpdate()
    {
    }

    protected virtual bool Initialize()
    {
        if (!Glfw.Init())
        {
            _logger.Error("Glfw: Unable to initialize");
            return false;
        }

        Glfw.SwapInterval(0);

        Glfw.WindowHint(Glfw.WindowInitHint.ClientApi, Glfw.ClientApi.OpenGL);
        Glfw.WindowHint(Glfw.WindowInitHint.IsResizeable, true);
        Glfw.WindowHint(Glfw.WindowInitHint.ScaleToMonitor, false);
        Glfw.WindowHint(Glfw.WindowOpenGLContextHint.Profile, Glfw.OpenGLProfile.Core);
        Glfw.WindowHint(Glfw.WindowOpenGLContextHint.DebugContext, true);
        Glfw.WindowHint(Glfw.WindowOpenGLContextHint.VersionMajor, 4);
        Glfw.WindowHint(Glfw.WindowOpenGLContextHint.VersionMinor, 6);

        var primaryMonitorHandle = Glfw.GetPrimaryMonitor();
        var videoMode = Glfw.GetVideoMode(primaryMonitorHandle);
        var screenWidth = videoMode.Width;
        var screenHeight = videoMode.Height;

        _applicationContext.ScreenSize = new Point(screenWidth, screenHeight);
        var windowWidth = (int)(0.8 * videoMode.Width);
        var windowHeight = (int)(0.8 * videoMode.Height);

        _windowHandle = Glfw.CreateWindow(windowWidth, windowHeight, "EngineKit", IntPtr.Zero, IntPtr.Zero);
        if (_windowHandle == IntPtr.Zero)
        {
            _logger.Error("Glfw: Unable to create window");
            return false;
        }

        Glfw.SetWindowPos(_windowHandle,  screenWidth / 2 - windowWidth / 2, screenHeight / 2 - windowHeight / 2);

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

    protected virtual void HandleDebugger(out bool breakOnError)
    {
        breakOnError = false;
    }

    protected virtual void WindowResized(int width, int height)
    {
    }

    protected virtual void FramebufferResized(int width, int height)
    {
    }

    protected bool IsKeyPressed(Glfw.Key key)
    {
        return Glfw.GetKeyPressed(_windowHandle, key);
    }

    protected bool IsMousePressed(Glfw.MouseButton mouseButton)
    {
        return _inputProvider.MouseState.IsButtonDown(mouseButton);
    }

    protected void Close()
    {
        Glfw.SetWindowShouldClose(_windowHandle, 1);
    }

    private void BindCallbacks()
    {
        _debugProcCallback = DebugCallback;
        _cursorEnterLeaveCallback = OnMouseEnterLeave;
        _cursorPositionCallback = OnMouseMove;
        _keyCallback = OnKey;
        _mouseButtonCallback = OnMouseButton;
        _framebufferSizeCallback = OnFramebufferSize;
        _windowSizeCallback = OnWindowSize;

        Glfw.SetKeyCallback(_windowHandle, _keyCallback);
        Glfw.SetCursorPositionCallback(_windowHandle, _cursorPositionCallback);
        Glfw.SetCursorEnterCallback(_windowHandle, _cursorEnterLeaveCallback);
        Glfw.SetMouseButtonCallback(_windowHandle, _mouseButtonCallback);
        Glfw.SetWindowSizeCallback(_windowHandle, _windowSizeCallback);
        Glfw.SetFramebufferSizeCallback(_windowHandle, _framebufferSizeCallback);
    }

    private void UnbindCallbacks()
    {
        _debugProcCallback = null;
        Glfw.SetCharCallback(_windowHandle, null);
        Glfw.SetKeyCallback(_windowHandle, null);
        Glfw.SetCursorEnterCallback(_windowHandle, null);
        Glfw.SetCursorPositionCallback(_windowHandle, null);
        Glfw.SetMouseButtonCallback(_windowHandle, null);
        Glfw.SetFramebufferSizeCallback(_windowHandle, null);
        Glfw.SetWindowSizeCallback(_windowHandle, null);
    }

    private void OnKey(
        IntPtr windowHandle,
        Glfw.Key key,
        Glfw.Scancode scancode,
        Glfw.KeyAction action,
        Glfw.KeyModifiers modifiers)
    {
        _logger.Debug("key: {Key} scancode: {ScanCode} action: {Action} modifiers: {Modifiers}", key, scancode, action,
            modifiers);
    }

    private static bool _isFirstFrame = true;

    private void OnMouseMove(
        IntPtr windowHandle,
        double currentCursorX,
        double currentCursorY)
    {
        if (_isFirstFrame)
        {
            _inputProvider.MouseState.PreviousX = (float)currentCursorX;
            _inputProvider.MouseState.PreviousY = (float)currentCursorY;
            _isFirstFrame = false;
        }

        _inputProvider.MouseState.X = (float)currentCursorX;
        _inputProvider.MouseState.Y = (float)currentCursorY;
        _inputProvider.MouseState.DeltaX = (float)currentCursorX - _inputProvider.MouseState.PreviousX;
        _inputProvider.MouseState.DeltaY = _inputProvider.MouseState.PreviousY - (float)currentCursorY;
        _inputProvider.MouseState.PreviousX = (float)currentCursorX;
        _inputProvider.MouseState.PreviousY = (float)currentCursorY;

        _logger.Debug("App: MouseMove: {MouseState}", _inputProvider.MouseState);
    }

    private void OnMouseEnterLeave(
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
        _inputProvider.MouseState[mouseButton] = action is Glfw.KeyAction.Pressed or Glfw.KeyAction.Repeat;
        _logger.Debug("button: {MouseButton} action: {Action} modifiers: {Modifiers}", mouseButton, action, modifiers);
    }

    private void OnWindowSize(
        IntPtr windowHandle,
        int width,
        int height)
    {
        WindowResized(width, height);
    }

    private void OnFramebufferSize(IntPtr windowHandle, int width, int height)
    {
        FramebufferResized(width, height);
    }

    private void DebugCallback(
        GL.DebugSource source,
        GL.DebugType type,
        uint id,
        GL.DebugSeverity severity,
        int length,
        IntPtr message,
        IntPtr userParam)
    {
        if (type == GL.DebugType.Portability)
        {
            return;
        }
        var messageString = Marshal.PtrToStringAnsi(message, length);

        switch (severity)
        {
            case GL.DebugSeverity.Notification or GL.DebugSeverity.DontCare:
                _logger.Debug("GL: {@Type} | {@MessageString}", type, messageString);
                break;
            case GL.DebugSeverity.High:
                _logger.Error("GL: {@Type} | {@MessageString}", type, messageString);
                break;
            case GL.DebugSeverity.Medium:
                _logger.Warning("GL: {@Type} | {@MessageString}", type, messageString);
                break;
            case GL.DebugSeverity.Low:
                _logger.Information("GL: {@Type} | {@MessageString}", type, messageString);
                break;
        }

        if (type == GL.DebugType.Error)
        {
            _logger.Error("{@MessageString}", messageString);

            HandleDebugger(out var breakOnError);
            if (breakOnError)
            {
                Debugger.Break();
            }
        }
    }
}