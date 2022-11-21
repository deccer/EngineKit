using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using EngineKit.Input;
using EngineKit.Native.Glfw;
using EngineKit.Native.OpenGL;
using Microsoft.Extensions.Options;
using OpenTK.Mathematics;
using Serilog;

namespace EngineKit;

public class Application : IApplication
{
    private readonly IOptions<WindowSettings> _windowSettings;
    private readonly IOptions<ContextSettings> _contextSettings;
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
        IOptions<WindowSettings> windowSettings,
        IOptions<ContextSettings> contextSettings,
        IApplicationContext applicationContext,
        IMetrics metrics,
        IInputProvider inputProvider)
    {
        _logger = logger.ForContext<Application>();
        _windowSettings = windowSettings;
        _contextSettings = contextSettings;
        _applicationContext = applicationContext;
        _metrics = metrics;
        _inputProvider = inputProvider;
        _windowHandle = IntPtr.Zero;
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

        _logger.Debug("{Category}: Initialized", "App");

        if (!Load())
        {
            return;
        }

        _logger.Debug("{Category}: Loaded", "App");

        var stopwatch = Stopwatch.StartNew();
        var accumulator = 0f;
        var currentTime = stopwatch.ElapsedMilliseconds;
        var lastTime = currentTime;
        var nextUpdate = lastTime + _showUpdatesPerSecondRate;
        _metrics.UpdateRate = 1.0f / 60.0f;

        while (!Glfw.ShouldWindowClose(_windowHandle))
        {
            _inputProvider.MouseState.Center();
            Glfw.PollEvents();

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
                _logger.Debug("{Category}: FPS: {@FramesPerSecond} UPS: {@UpdatesPerSecond} UR: {@UpdateRate} SD: {SwapBufferDuration:F3}ms",
                    "App",
                    _metrics.FramesPerSecond,
                    _metrics.UpdatesPerSecond,
                    _metrics.UpdateRate,
                    _metrics.SwapBufferDuration);
                _metrics.UpdatesPerSecond = 0;
                _metrics.FramesPerSecond = 0;
                nextUpdate = stopwatch.ElapsedMilliseconds + _showUpdatesPerSecondRate;
            }
        }

        _logger.Debug("{Category}: Unloading", "App");

        Unload();

        _logger.Debug("{Category}: Unloaded", "App");
    }

    protected virtual void FixedUpdate()
    {
    }

    protected virtual bool Initialize()
    {
        PrintSystemInformation();

        if (!Glfw.Init())
        {
            _logger.Error("{Category}: Unable to initialize", "Glfw");
            return false;
        }
        
        Glfw.SetErrorCallback(ErrorCallback);

        var windowSettings = _windowSettings.Value;
        var windowResizable = windowSettings.WindowMode is WindowMode.Windowed or WindowMode.WindowedBorderless;

        Glfw.WindowHint(Glfw.WindowInitHint.ScaleToMonitor, true);
        Glfw.WindowHint(Glfw.WindowInitHint.ClientApi, Glfw.ClientApi.OpenGL);
        Glfw.WindowHint(Glfw.WindowInitHint.IsResizeable, windowResizable);
        Glfw.WindowHint(Glfw.WindowInitHint.IsDecorated, windowSettings.WindowMode != WindowMode.WindowedBorderless);
        Glfw.WindowHint(Glfw.WindowInitHint.IsMaximized, !windowResizable);
        Glfw.WindowHint(Glfw.WindowInitHint.IsFloating, !windowResizable);
        Glfw.WindowHint(Glfw.WindowInitHint.IsFocused, true);

        var monitorHandle = Glfw.GetPrimaryMonitor();
        var videoMode = Glfw.GetVideoMode(monitorHandle);
        var screenWidth = videoMode.Width;
        var screenHeight = videoMode.Height;

        _applicationContext.ScreenSize = new Vector2i(screenWidth, screenHeight);
        _applicationContext.WindowSize = windowResizable
            ? new Vector2i(_windowSettings.Value.ResolutionWidth,  _windowSettings.Value.ResolutionHeight)
            : new Vector2i((int)(screenWidth * 0.8f), (int)(screenHeight * 0.8f));

        if (!windowResizable)
        {
            _applicationContext.WindowSize = new Vector2i(screenWidth, screenHeight);
        }
        monitorHandle = windowResizable || windowSettings.WindowMode == WindowMode.WindowedFullscreen
            ? IntPtr.Zero
            : monitorHandle;

        var glVersion = new Version(4, 5);
        if (!string.IsNullOrEmpty(_contextSettings.Value.TargetGLVersion))
        {
            if (!Version.TryParse(_contextSettings.Value.TargetGLVersion, out glVersion))
            {
                _logger.Error("{Category}: Unable to detect context version. Assuming 4.5", "App");
                glVersion = new Version(4, 5);
            }
        }

        Glfw.WindowHint(Glfw.WindowOpenGLContextHint.VersionMajor, glVersion.Major);
        Glfw.WindowHint(Glfw.WindowOpenGLContextHint.VersionMinor, glVersion.Minor);
        Glfw.WindowHint(Glfw.WindowOpenGLContextHint.DebugContext, _contextSettings.Value.IsDebugContext);
        Glfw.WindowHint(Glfw.WindowOpenGLContextHint.Profile, Glfw.OpenGLProfile.Core);

        // MESA overrides - useful for windows on intel igpu
        var environmentVariables = Environment.GetEnvironmentVariables();
        if (environmentVariables.Contains("LIBGL_DEBUG"))
        {
            var libGlDebug = environmentVariables["LIBGL_DEBUG"];
            _logger.Information("{Category}: LIBGL_DEBUG={LibGlDebug}", "Environment", libGlDebug);
        }

        if (environmentVariables.Contains("MESA_GL_VERSION_OVERRIDE"))
        {
            var mesaGlVersionOverride = environmentVariables["MESA_GL_VERSION_OVERRIDE"];
            _logger.Information("{Category}: MESA_GL_VERSION_OVERRIDE={MesaGlVersionOverride}", "Environment", mesaGlVersionOverride);
        }

        if (environmentVariables.Contains("MESA_GLSL_VERSION_OVERRIDE"))
        {
            var mesaGlslVersionOverride = environmentVariables["MESA_GLSL_VERSION_OVERRIDE"];
            _logger.Information("{Category}: MESA_GLSL_VERSION_OVERRIDE={MesaGlVersionOverride}", "Environment", mesaGlslVersionOverride);
        }
        _windowHandle = Glfw.CreateWindow(
            _applicationContext.WindowSize.X,
            _applicationContext.WindowSize.Y,
            "EngineKit",
            monitorHandle,
            IntPtr.Zero);
        if (_windowHandle == IntPtr.Zero)
        {
            _logger.Error("{Category}: Unable to create window", "Glfw");
            return false;
        }

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            /* From GLFW: Due to the asynchronous nature of X11, it may take
             *  a moment for a window to reach its requested state.  This means you may not
             *  be able to query the final size, position or other attributes directly after
             *  window creation.
             */
            Glfw.PollEvents();
            Glfw.WaitEventsTimeout(0.5);
            Glfw.PollEvents();
            Glfw.SwapBuffers(_windowHandle);
            Glfw.WaitEventsTimeout(0.5);
        }

        if (windowResizable)
        {
            Glfw.SetWindowPos(
                _windowHandle,
                screenWidth / 2 - _applicationContext.WindowSize.X / 2,
                screenHeight / 2 - _applicationContext.WindowSize.Y / 2);
        }
        else
        {
            Glfw.SetWindowPos(_windowHandle, 0,0);
        }

        Glfw.GetFramebufferSize(
            _windowHandle,
            out var framebufferWidth,
            out var framebufferHeight);
        _applicationContext.FramebufferSize = new Vector2i(
            framebufferWidth,
            framebufferHeight);
        _applicationContext.ScaledFramebufferSize = new Vector2i(
            (int)(framebufferWidth * _windowSettings.Value.ResolutionScale),
            (int)(framebufferHeight * _windowSettings.Value.ResolutionScale));

        Glfw.MakeContextCurrent(_windowHandle);

        _logger.Information("{Category}: Vendor - {Vendor}", "GL", GL.GetString(GL.StringName.Vendor));
        _logger.Information("{Category}: Renderer - {Renderer}", "GL", GL.GetString(GL.StringName.Renderer));
        _logger.Information("{Category}: Version - {Version}", "GL", GL.GetString(GL.StringName.Version));
        _logger.Information("{Category}: Shading Language Version - {ShadingLanguageVersion}", "GL", GL.GetString(GL.StringName.ShadingLanguageVersion));

        Glfw.SwapInterval(_windowSettings.Value.IsVsyncEnabled ? 1 : 0);

        BindCallbacks();

        if (_contextSettings.Value.IsDebugContext && _debugProcCallback != null)
        {
            _logger.Debug("{Category}: Debug callback enabled", "GL");
            GL.DebugMessageCallback(_debugProcCallback, IntPtr.Zero);
            GL.Enable(GL.EnableType.DebugOutput);
            GL.Enable(GL.EnableType.DebugOutputSynchronous);
        }
        else
        {
            _logger.Debug("{Category}: Debug callback disabled", "GL");
        }

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

    protected virtual void WindowResized()
    {
    }

    protected virtual void FramebufferResized()
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
        if (key != Glfw.Key.Unknown)
        {
            _inputProvider.KeyboardState.SetKeyState(key, action is Glfw.KeyAction.Pressed or Glfw.KeyAction.Repeat);
        }

        /*
        _logger.Debug("{Category} Key: {Key} Scancode: {ScanCode} Action: {Action} Modifiers: {Modifiers}",
            "Glfw",
            key,
            scancode,
            action,
            modifiers);
            */
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

        //_logger.Debug("{Category}: MouseMove: {MouseState}", "Glfw", _inputProvider.MouseState);
    }

    private void OnMouseEnterLeave(
        IntPtr windowHandle,
        Glfw.CursorEnterMode cursorEnterMode)
    {
        _logger.Debug("{Category}: Mode: {CursorEnterMode}", "Glfw", cursorEnterMode);
    }

    private void OnMouseButton(
        IntPtr windowHandle,
        Glfw.MouseButton mouseButton,
        Glfw.KeyAction action,
        Glfw.KeyModifiers modifiers)
    {
        _inputProvider.MouseState[mouseButton] = action is Glfw.KeyAction.Pressed or Glfw.KeyAction.Repeat;
        _logger.Debug("{Category}: Button: {MouseButton} Action: {Action} Modifiers: {Modifiers}",
            "Glfw",
            mouseButton,
            action,
            modifiers);
    }

    private void OnWindowSize(
        IntPtr windowHandle,
        int width,
        int height)
    {
        _applicationContext.WindowSize = new Vector2i(width, height);
        WindowResized();
    }

    private void OnFramebufferSize(IntPtr windowHandle, int width, int height)
    {
        _applicationContext.FramebufferSize = new Vector2i(width, height);
        FramebufferResized();
    }

    private void PrintSystemInformation()
    {
        _logger.Debug("{Category}: Architecture - {@OSArchitecture}", "OS", RuntimeInformation.OSArchitecture);
        _logger.Debug("{Category}: Description - {@OSDescription}", "OS", RuntimeInformation.OSDescription);

        _logger.Debug("{Category}: Framework - {@FrameworkDescription}", "RT", RuntimeInformation.FrameworkDescription);
        _logger.Debug("{Category}: Runtime Identifier - {@RuntimeIdentifier}", "RT", RuntimeInformation.RuntimeIdentifier);
        _logger.Debug("{Category}: Process Architecture - {@ProcessArchitecture}", "RT", RuntimeInformation.ProcessArchitecture);
    }

    private void DebugCallback(
        GL.DebugSource source,
        GL.DebugType type,
        uint id,
        GL.DebugSeverity severity,
        int length,
        IntPtr messagePtr,
        IntPtr userParam)
    {
        if (type is GL.DebugType.Portability or GL.DebugType.PushGroup or GL.DebugType.PopGroup)
        {
            return;
        }

        var message = Marshal.PtrToStringAnsi(messagePtr, length);

        switch (severity)
        {
            case GL.DebugSeverity.Notification or GL.DebugSeverity.DontCare:
                _logger.Debug("{Category}: {@Type} | {@MessageString}", "GL", type, message);
                break;
            case GL.DebugSeverity.High:
                _logger.Error("{Category}: {@Type} | {@MessageString}", "GL", type, message);
                break;
            case GL.DebugSeverity.Medium:
                _logger.Warning("{Category}: {@Type} | {@MessageString}", "GL", type, message);
                break;
            case GL.DebugSeverity.Low:
                _logger.Information("{Category}: {@Type} | {@MessageString}", "GL", type, message);
                break;
        }

        if (type == GL.DebugType.Error)
        {
            _logger.Error("{@MessageString}", message);

            HandleDebugger(out var breakOnError);
            if (breakOnError)
            {
                Debugger.Break();
            }
        }
    }
    
    private void ErrorCallback(Glfw.ErrorCode errorCode, string errorDescription)
    {
        _logger.Error("{Category}: {ErrorCode} - {ErrorDescription}", "Glfw", errorCode, errorDescription);
    }
}