using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using EngineKit.Input;
using EngineKit.Mathematics;
using EngineKit.Native.Glfw;
using EngineKit.Native.Ktx;
using EngineKit.Native.OpenGL;
using Microsoft.Extensions.Options;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Point = EngineKit.Mathematics.Point;

namespace EngineKit;

public class Application : IApplication
{
    private readonly IOptions<WindowSettings> _windowSettings;
    private readonly IOptions<ContextSettings> _contextSettings;
    private readonly IApplicationContext _applicationContext;
    private readonly ICapabilities _capabilities;
    private readonly ILimits _limits;
    private readonly IMetrics _metrics;
    private readonly IInputProvider _inputProvider;
    private readonly ILogger _logger;
    private nint _windowHandle;

    private GL.GLDebugProc? _debugProcCallback;
    private Glfw.FramebufferSizeCallback? _framebufferSizeCallback;
    private Glfw.KeyCallback? _keyCallback;
    private Glfw.MouseButtonCallback? _mouseButtonCallback;
    private Glfw.ScrollCallback? _mouseScrollCallback;
    private Glfw.CursorEnterCallback? _cursorEnterLeaveCallback;
    private Glfw.CursorPositionCallback? _cursorPositionCallback;
    private Glfw.WindowSizeCallback? _windowSizeCallback;

    private static bool _isFirstFrame = true;
    private bool _isCursorVisible;

    private readonly FrameTimeAverager _frameTimeAverager;
    private long _previousFrameTicks;

    protected Application(
        ILogger logger,
        IOptions<WindowSettings> windowSettings,
        IOptions<ContextSettings> contextSettings,
        IApplicationContext applicationContext,
        ICapabilities capabilities,
        ILimits limits,
        IMetrics metrics,
        IInputProvider inputProvider)
    {
        _logger = logger.ForContext<Application>();
        _windowSettings = windowSettings;
        _contextSettings = contextSettings;
        _applicationContext = applicationContext;
        _capabilities = capabilities;
        _limits = limits;
        _metrics = metrics;
        _inputProvider = inputProvider;
        _windowHandle = nint.Zero;
        _frameTimeAverager = new FrameTimeAverager(50);
    }
    
    public double DesiredFramerate { get; set; } = 120.0;
    
    public bool LimitFrameRate { get; set; } = true;

    public void Dispose()
    {
        Ktx.Terminate();
    }
    
    public void Run()
    {
        if (!Initialize())
        {
            return;
        }

        if (!Ktx.Init())
        {
            _logger.Debug("{Category}: Unable to initialize Ktx2", "App");
            return;
        }

        _logger.Debug("{Category}: Initialized", "App");

        if (!Load())
        {
            return;
        }

        _logger.Debug("{Category}: Loaded", "App");
        
        if (Glfw.IsRawMouseMotionSupported())
        {
            Glfw.SetInputMode(_windowHandle, Glfw.InputMode.RawMouseMotion, Glfw.True);
        }
        
        var stopwatch = Stopwatch.StartNew();        
        while (!Glfw.ShouldWindowClose(_windowHandle))
        {
            var desiredFrameTime = 1000.0 / DesiredFramerate;
            var currentFrameTicks = stopwatch.ElapsedTicks;
            var deltaMilliseconds = (currentFrameTicks - _previousFrameTicks) * (1000.0 / Stopwatch.Frequency);

            while (LimitFrameRate && deltaMilliseconds < desiredFrameTime)
            {
                Thread.Sleep(0);
                currentFrameTicks = stopwatch.ElapsedTicks;
                deltaMilliseconds = (currentFrameTicks - _previousFrameTicks) * (1000.0 / Stopwatch.Frequency);
            }
            _previousFrameTicks = currentFrameTicks;
            var deltaSeconds = (float)deltaMilliseconds / 1000.0f;
            _frameTimeAverager.AddTime(deltaMilliseconds);
            _metrics.AverageFrameTime = _frameTimeAverager.CurrentAverageFrameTime;

            Render(deltaSeconds);
            Update(deltaSeconds);

            _inputProvider.MouseState.Center();
            Glfw.PollEvents();
            Glfw.SwapBuffers(_windowHandle);            

            _metrics.FrameCounter++;
        }

        stopwatch.Stop();

        _logger.Debug("{Category}: Unloading", "App");

        Unload();
        Glfw.DestroyWindow(_windowHandle);

        _logger.Debug("{Category}: Unloaded", "App");
        Glfw.Terminate();
    }

    protected void CenterMouseCursor()
    {
        Glfw.SetCursorPos(_windowHandle, _applicationContext.WindowSize.X / 2, _applicationContext.WindowSize.Y / 2);
    }

    protected bool IsCursorVisible()
    {
        return _isCursorVisible;
    }

    protected void HideCursor()
    {
        _isCursorVisible = false;
        Glfw.SetInputMode(_windowHandle, Glfw.InputMode.Cursor, Glfw.CursorHidden);
    }

    protected void ShowCursor()
    {
        _isCursorVisible = true;
        Glfw.SetInputMode(_windowHandle, Glfw.InputMode.Cursor, Glfw.CursorNormal);
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
        Glfw.WindowHint(Glfw.WindowInitHint.IsResizeable, windowResizable);
        Glfw.WindowHint(Glfw.WindowInitHint.IsDecorated, windowSettings.WindowMode == WindowMode.Windowed);
        Glfw.WindowHint(Glfw.WindowInitHint.IsMaximized, !windowResizable);
        Glfw.WindowHint(Glfw.WindowInitHint.IsFloating, !windowResizable);
        Glfw.WindowHint(Glfw.WindowInitHint.IsFocused, true);
        /*
        Glfw.WindowHint(Glfw.FramebufferInitHint.RedBits, 10);
        Glfw.WindowHint(Glfw.FramebufferInitHint.GreenBits, 10);
        Glfw.WindowHint(Glfw.FramebufferInitHint.BlueBits, 10);
        Glfw.WindowHint(Glfw.FramebufferInitHint.AlphaBits, 2);
        */
        var monitorHandle = Glfw.GetPrimaryMonitor();
        var videoMode = Glfw.GetVideoMode(monitorHandle);
        var screenWidth = videoMode.Width;
        var screenHeight = videoMode.Height;
        DesiredFramerate = videoMode.RefreshRate;

        Glfw.GetMonitorPos(
            monitorHandle,
            out var monitorLeft,
            out var monitorTop);

        _applicationContext.ScreenSize = new Point(screenWidth, screenHeight);
        _applicationContext.WindowSize = windowResizable
            ? new Point(_windowSettings.Value.ResolutionWidth,  _windowSettings.Value.ResolutionHeight)
            : new Point((int)(screenWidth * 0.9f), (int)(screenHeight * 0.9f));

        if (!windowResizable)
        {
            _applicationContext.WindowSize = new Point(screenWidth, screenHeight);
        }
        monitorHandle = windowResizable || windowSettings.WindowMode == WindowMode.WindowedFullscreen
            ? nint.Zero
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

        Glfw.WindowHint(Glfw.WindowInitHint.ClientApi, Glfw.ClientApi.OpenGL);
        Glfw.WindowHint(Glfw.WindowOpenGLContextHint.VersionMajor, glVersion.Major);
        Glfw.WindowHint(Glfw.WindowOpenGLContextHint.VersionMinor, glVersion.Minor);
        Glfw.WindowHint(Glfw.WindowOpenGLContextHint.DebugContext, _contextSettings.Value.IsDebugContext);
        Glfw.WindowHint(Glfw.WindowOpenGLContextHint.Profile, Glfw.OpenGLProfile.Core);

        // MESA overrides - useful for windows on intel iGPU
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
            "OpenSpace",
            monitorHandle,
            nint.Zero);
        if (_windowHandle == nint.Zero)
        {
            _logger.Error("{Category}: Unable to create window", "Glfw");
            return false;
        }
        
        Glfw.SwapBuffers(_windowHandle);

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            /* From GLFW: Due to the asynchronous nature of X11, it may take
             *  a moment for a window to reach its requested state.  This means you may not
             *  be able to query the final size, position or other attributes directly after
             *  window creation.
             */
            Glfw.PollEvents();
            Glfw.WaitEventsTimeout(0.25);
            Glfw.PollEvents();
            Glfw.SwapBuffers(_windowHandle);
            Glfw.WaitEventsTimeout(0.25);
        }

        if (windowResizable)
        {
            Glfw.SetWindowPos(
                _windowHandle,
                screenWidth / 2 - _applicationContext.WindowSize.X / 2 + monitorLeft,
                screenHeight / 2 - _applicationContext.WindowSize.Y / 2 + monitorTop);
        }
        else
        {
            Glfw.SetWindowPos(_windowHandle, monitorLeft, monitorTop);
        }

        Glfw.GetFramebufferSize(
            _windowHandle,
            out var framebufferWidth,
            out var framebufferHeight);
        _applicationContext.FramebufferSize = new Point(
            framebufferWidth,
            framebufferHeight);
        _applicationContext.ScaledFramebufferSize = new Point(
            (int)(framebufferWidth * _windowSettings.Value.ResolutionScale),
            (int)(framebufferHeight * _windowSettings.Value.ResolutionScale));

        if (Glfw.IsRawMouseMotionSupported())
        {
            Glfw.SetInputMode(_windowHandle, Glfw.InputMode.RawMouseMotion, Glfw.True);
        }

        Glfw.MakeContextCurrent(_windowHandle);

        if (!_capabilities.Load())
        {
            return false;
        }

        if (!_limits.Load())
        {
            return false;
        }

        _logger.Information("{Category}: Vendor - {Vendor}", "GL", GL.GetString(GL.StringName.Vendor));
        _logger.Information("{Category}: Renderer - {Renderer}", "GL", GL.GetString(GL.StringName.Renderer));
        _logger.Information("{Category}: Version - {Version}", "GL", GL.GetString(GL.StringName.Version));
        _logger.Information("{Category}: Shading Language Version - {ShadingLanguageVersion}", "GL", GL.GetString(GL.StringName.ShadingLanguageVersion));

        
        if (_capabilities.SupportsSwapControl)
        {
            Glfw.SwapInterval(_windowSettings.Value.IsVsyncEnabled ? 1 : -1);
        }
        else
        {
            Glfw.SwapInterval(_windowSettings.Value.IsVsyncEnabled ? 1 : 0);
        }

        LimitFrameRate = _windowSettings.Value.IsVsyncEnabled;

        BindCallbacks();

        if (_contextSettings.Value.IsDebugContext && _debugProcCallback != null)
        {
            _logger.Debug("{Category}: Debug callback enabled", "GL");
            GL.DebugMessageCallback(_debugProcCallback, nint.Zero);
            GL.Enable(GL.EnableType.DebugOutput);
            GL.Enable(GL.EnableType.DebugOutputSynchronous);
        }
        else
        {
            _logger.Debug("{Category}: Debug callback disabled", "GL");
        }
        
        GL.Enable(GL.EnableType.FramebufferSrgb);

        return true;
    }

    protected virtual bool Load()
    {
        return true;
    }

    protected virtual void Render(float deltaTime)
    {
    }

    protected virtual void Unload()
    {
        UnbindCallbacks();
    }

    protected virtual void Update(float deltaTime)
    {
    }

    protected void SetWindowIcon(Image<Rgba32> image)
    {
        unsafe
        {
            if (image.DangerousTryGetSinglePixelMemory(out var memory))
            {
                Glfw.SetWindowIcon(_windowHandle, new Glfw.Image
                {
                    Width = image.Width,
                    Height = image.Height,
                    PixelPtr = (byte*)memory.Pin().Pointer
                });
            }
        }
    }

    protected void SetWindowIcon(string fileName)
    {
        if (!File.Exists(fileName))
        {
            _logger.Error("{Category}: Window icon file {FilePath} not found", "App", fileName);
            return;
        }
        
        unsafe
        {
            using var image = Image.Load<Rgba32>(fileName);
            if (image.DangerousTryGetSinglePixelMemory(out var memory))
            {
                Glfw.SetWindowIcon(_windowHandle, new Glfw.Image
                {
                    Width = image.Width,
                    Height = image.Height,
                    PixelPtr = (byte*)memory.Pin().Pointer
                });
            }
        }
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
        _mouseScrollCallback = OnMouseScroll;
        _framebufferSizeCallback = OnFramebufferSize;
        _windowSizeCallback = OnWindowSize;

        Glfw.SetKeyCallback(_windowHandle, _keyCallback);
        Glfw.SetCursorPositionCallback(_windowHandle, _cursorPositionCallback);
        Glfw.SetCursorEnterCallback(_windowHandle, _cursorEnterLeaveCallback);
        Glfw.SetMouseButtonCallback(_windowHandle, _mouseButtonCallback);
        Glfw.SetScrollCallback(_windowHandle, _mouseScrollCallback);
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
        Glfw.SetScrollCallback(_windowHandle, null);
        Glfw.SetFramebufferSizeCallback(_windowHandle, null);
        Glfw.SetWindowSizeCallback(_windowHandle, null);
    }

    private void OnKey(
        nint windowHandle,
        Glfw.Key key,
        Glfw.Scancode scancode,
        Glfw.KeyAction action,
        Glfw.KeyModifiers modifiers)
    {
        if (key != Glfw.Key.Unknown)
        {
            _inputProvider.KeyboardState.SetKeyState(key, action is Glfw.KeyAction.Pressed or Glfw.KeyAction.Repeat);
        }

        _logger.Verbose("{Category} Key: {Key} Scancode: {ScanCode} Action: {Action} Modifiers: {Modifiers}",
            "Glfw",
            key,
            scancode,
            action,
            modifiers);
    }

    private void OnMouseMove(
        nint windowHandle,
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
        _inputProvider.MouseState.DeltaX += (float)currentCursorX - _inputProvider.MouseState.PreviousX;
        _inputProvider.MouseState.DeltaY += _inputProvider.MouseState.PreviousY - (float)currentCursorY;
        _inputProvider.MouseState.PreviousX = (float)currentCursorX;
        _inputProvider.MouseState.PreviousY = (float)currentCursorY;

        //_logger.Verbose("{Category}: MouseMove: {MouseState}", "Glfw", _inputProvider.MouseState);
    }

    private void OnMouseEnterLeave(
        nint windowHandle,
        Glfw.CursorEnterMode cursorEnterMode)
    {
        _logger.Verbose("{Category}: Mode: {CursorEnterMode}", "Glfw", cursorEnterMode);
    }

    private void OnMouseButton(
        nint windowHandle,
        Glfw.MouseButton mouseButton,
        Glfw.KeyAction action,
        Glfw.KeyModifiers modifiers)
    {
        _inputProvider.MouseState[mouseButton] = action is Glfw.KeyAction.Pressed or Glfw.KeyAction.Repeat;
        _logger.Verbose("{Category}: Button: {MouseButton} Action: {Action} Modifiers: {Modifiers}",
            "Glfw",
            mouseButton,
            action,
            modifiers);
    }

    private void OnMouseScroll(
        nint windowHandle,
        double scrollX,
        double scrollY)
    {
        _inputProvider.MouseState.Scroll += new Vector2((float)scrollX, (float)scrollY);
    }

    private void OnWindowSize(
        nint windowHandle,
        int width,
        int height)
    {
        var oldWindowSize = _applicationContext.WindowSize;
        _applicationContext.WindowSize = new Point(width, height);
        if (_applicationContext.ShowResizeInLog)
        {
            _logger.Debug("{Category}: Window resized from {OldWidth}x{OldHeight} to {Width}x{Height}", "App", oldWindowSize.X, oldWindowSize.Y, width, height);
        }
        WindowResized();
    }

    private void OnFramebufferSize(nint windowHandle, int width, int height)
    {
        var oldFramebufferSize = _applicationContext.FramebufferSize;
        var oldScaledFramebufferSize = _applicationContext.ScaledFramebufferSize;
        _applicationContext.FramebufferSize = new Point(width, height);

        _applicationContext.ScaledFramebufferSize = new Point(
            (int)(width * _windowSettings.Value.ResolutionScale),
            (int)(height * _windowSettings.Value.ResolutionScale));

        if (_applicationContext.ShowResizeInLog)
        {
            _logger.Debug("{Category}: Framebuffer resized from {OldWidth}x{OldHeight} to {Width}x{Height}", "App", oldFramebufferSize.X, oldFramebufferSize.Y, width, height);
            _logger.Debug("{Category}: ScaledFramebuffer resized from {OldWidth}x{OldHeight} to {Width}x{Height}", "App", oldScaledFramebufferSize.X, oldScaledFramebufferSize.Y, width, height);
        }
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
        nint messagePtr,
        nint userParam)
    {
        if (type is GL.DebugType.Portability or GL.DebugType.Other or GL.DebugType.PushGroup or GL.DebugType.PopGroup)
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