using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using EngineKit.Core;
using EngineKit.Core.Messages;
using EngineKit.Input;
using EngineKit.Mathematics;
using EngineKit.Native.Glfw;
using EngineKit.Native.Ktx;
using EngineKit.Native.OpenGL;
//using Ktx2Sharp;
using Microsoft.Extensions.Options;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace EngineKit;

public class Application : IApplication
{
    private readonly IOptions<WindowSettings> _windowSettings;
    private readonly IOptions<ContextSettings> _contextSettings;
    private readonly IApplicationContext _applicationContext;
    private readonly ICapabilities _capabilities;
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
    private Glfw.CharCallback? _windowCharCallback;

    private static bool _isFirstFrame = true;
    private bool _isCursorVisible;

    private readonly FrameTimeAverager _frameTimeAverager;
    private long _previousFrameTicks;

    protected Application(ILogger logger,
                          IOptions<WindowSettings> windowSettings,
                          IOptions<ContextSettings> contextSettings,
                          IApplicationContext applicationContext,
                          IMessageBus messageBus,
                          ICapabilities capabilities,
                          IMetrics metrics,
                          IInputProvider inputProvider)
    {
        _logger = logger.ForContext<Application>();
        _windowSettings = windowSettings;
        _contextSettings = contextSettings;
        _applicationContext = applicationContext;
        _capabilities = capabilities;
        _metrics = metrics;
        _inputProvider = inputProvider;
        _windowHandle = nint.Zero;
        _frameTimeAverager = new FrameTimeAverager(60);

        messageBus.Subscribe<CloseWindowMessage>(OnMessageCloseWindow);
        messageBus.Subscribe<RestoreWindowMessage>(OnMessageRestoreWindow);
        messageBus.Subscribe<MaximizeWindowMessage>(OnMessageMaximizeWindow);
    }

    public void Dispose()
    {
        Ktx.Terminate();
    }

    public void Run()
    {
        if(!OnInitialize())
        {
            return;
        }

        if(!Ktx.Init())
        {
            _logger.Debug("{Category}: Unable to initialize Ktx2", "App");

            return;
        }

        _logger.Debug("{Category}: Initialized", "App");

        if(!OnLoad())
        {
            return;
        }

        _logger.Debug("{Category}: Loaded", "App");

        if(Glfw.IsRawMouseMotionSupported())
        {
            Glfw.SetInputMode(_windowHandle, Glfw.InputMode.RawMouseMotion, Glfw.True);
        }

        var stopwatch = Stopwatch.StartNew();

        while(!Glfw.ShouldWindowClose(_windowHandle))
        {
            var desiredFrameTime = 1000.0 / _applicationContext.DesiredFramerate;
            var currentFrameTicks = stopwatch.ElapsedTicks;
            var deltaMilliseconds = (currentFrameTicks - _previousFrameTicks) * (1000.0 / Stopwatch.Frequency);

            while(_applicationContext.IsFrameRateLimited && deltaMilliseconds < desiredFrameTime)
            {
                Thread.Sleep(0);
                currentFrameTicks = stopwatch.ElapsedTicks;
                deltaMilliseconds = (currentFrameTicks - _previousFrameTicks) * (1000.0f / Stopwatch.Frequency);
            }

            _previousFrameTicks = currentFrameTicks;
            var deltaSeconds = (float)deltaMilliseconds / 1000.0f;
            _frameTimeAverager.AddTime(deltaMilliseconds);
            _metrics.AverageFrameTime = _frameTimeAverager.CurrentAverageFrameTime;

            var elapsedSeconds = (float)stopwatch.Elapsed.TotalSeconds;
            OnRender(deltaSeconds, elapsedSeconds);
            OnUpdate(deltaSeconds, elapsedSeconds);

            _inputProvider.MouseState.Center();
            Glfw.PollEvents();
            Glfw.SwapBuffers(_windowHandle);

            _metrics.FrameCounter++;
        }

        stopwatch.Stop();

        _logger.Debug("{Category}: Unloading", "App");

        OnUnload();
        Glfw.DestroyWindow(_windowHandle);
        Glfw.Terminate();
        _logger.Debug("{Category}: Unloaded", "App");
    }

    protected void CenterMouseCursor()
    {
        Glfw.SetCursorPos(_windowHandle, _applicationContext.WindowSize.X / 2.0f, _applicationContext.WindowSize.Y / 2.0f);
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

    protected virtual void OnFixedUpdate()
    { }

    protected virtual bool OnInitialize()
    {
        PrintSystemInformation();

        if(!Glfw.Init())
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
        _applicationContext.DesiredFramerate = videoMode.RefreshRate;

        Glfw.GetMonitorPos(monitorHandle,
                           out var monitorLeft,
                           out var monitorTop);

        _applicationContext.ScreenSize = new Int2(screenWidth, screenHeight);

        _applicationContext.WindowSize = windowResizable
                                             ? new Int2(_windowSettings.Value.ResolutionWidth, _windowSettings.Value.ResolutionHeight)
                                             : new Int2((int)(screenWidth * 0.9f), (int)(screenHeight * 0.9f));

        if(!windowResizable)
        {
            _applicationContext.WindowSize = new Int2(screenWidth, screenHeight);
        }

        monitorHandle = windowResizable || windowSettings.WindowMode == WindowMode.WindowedFullscreen
                            ? nint.Zero
                            : monitorHandle;

        var glVersion = new Version(4, 6);

        if(!string.IsNullOrEmpty(_contextSettings.Value.TargetGLVersion))
        {
            if(!Version.TryParse(_contextSettings.Value.TargetGLVersion, out glVersion))
            {
                _logger.Error("{Category}: Unable to detect context version. Assuming 4.6", "App");
                glVersion = new Version(4, 6);
            }
        }

        Glfw.WindowHint(Glfw.WindowInitHint.ClientApi, Glfw.ClientApi.OpenGL);
        Glfw.WindowHint(Glfw.WindowOpenGLContextHint.VersionMajor, glVersion.Major);
        Glfw.WindowHint(Glfw.WindowOpenGLContextHint.VersionMinor, glVersion.Minor);
        Glfw.WindowHint(Glfw.WindowOpenGLContextHint.DebugContext, _contextSettings.Value.IsDebugContext);
        Glfw.WindowHint(Glfw.WindowOpenGLContextHint.Profile, Glfw.OpenGLProfile.Core);

        // show MESA overrides - useful for windows on intel iGPU
        var environmentVariables = Environment.GetEnvironmentVariables();

        if(environmentVariables.Contains("LIBGL_DEBUG"))
        {
            var libGlDebug = environmentVariables["LIBGL_DEBUG"];
            _logger.Information("{Category}: LIBGL_DEBUG={LibGlDebug}", "Environment", libGlDebug);
        }

        if(environmentVariables.Contains("MESA_GL_VERSION_OVERRIDE"))
        {
            var mesaGlVersionOverride = environmentVariables["MESA_GL_VERSION_OVERRIDE"];
            _logger.Information("{Category}: MESA_GL_VERSION_OVERRIDE={MesaGlVersionOverride}", "Environment", mesaGlVersionOverride);
        }

        if(environmentVariables.Contains("MESA_GLSL_VERSION_OVERRIDE"))
        {
            var mesaGlslVersionOverride = environmentVariables["MESA_GLSL_VERSION_OVERRIDE"];
            _logger.Information("{Category}: MESA_GLSL_VERSION_OVERRIDE={MesaGlVersionOverride}", "Environment", mesaGlslVersionOverride);
        }

        _windowHandle = Glfw.CreateWindow(_applicationContext.WindowSize.X,
                                          _applicationContext.WindowSize.Y,
                                          _windowSettings.Value.Title,
                                          monitorHandle,
                                          nint.Zero);

        if(_windowHandle == nint.Zero)
        {
            _logger.Error("{Category}: Unable to create window", "Glfw");

            return false;
        }

        Glfw.SwapBuffers(_windowHandle);

        if(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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

        if(windowResizable)
        {
            Glfw.SetWindowPos(_windowHandle,
                              screenWidth / 2 - _applicationContext.WindowSize.X / 2 + monitorLeft,
                              screenHeight / 2 - _applicationContext.WindowSize.Y / 2 + monitorTop);
        }
        else
        {
            Glfw.SetWindowPos(_windowHandle, monitorLeft, monitorTop);
        }

        Glfw.GetFramebufferSize(_windowHandle,
                                out var framebufferWidth,
                                out var framebufferHeight);

        _applicationContext.ResizeWindowFramebuffer(framebufferWidth, framebufferHeight);
        _applicationContext.IsWindowMaximized = false;

        if(Glfw.IsRawMouseMotionSupported())
        {
            Glfw.SetInputMode(_windowHandle, Glfw.InputMode.RawMouseMotion, Glfw.True);
        }

        Glfw.MakeContextCurrent(_windowHandle);

        if(!_capabilities.Load())
        {
            return false;
        }

        _logger.Information("{Category}: Vendor - {Vendor}", "GL", GL.GetString(GL.StringName.Vendor));
        _logger.Information("{Category}: Renderer - {Renderer}", "GL", GL.GetString(GL.StringName.Renderer));
        _logger.Information("{Category}: Version - {Version}", "GL", GL.GetString(GL.StringName.Version));
        _logger.Information("{Category}: Shading Language Version - {ShadingLanguageVersion}", "GL", GL.GetString(GL.StringName.ShadingLanguageVersion));

        Glfw.SwapInterval(_windowSettings.Value.IsVsyncEnabled
                              ? 1
                              : _capabilities.SupportsSwapControl
                                  ? -1
                                  : 0);

        _applicationContext.IsFrameRateLimited = _windowSettings.Value.IsVsyncEnabled;

        BindGlfwCallbacks();

        if(_contextSettings.Value.IsDebugContext && _debugProcCallback != null)
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

    protected virtual bool OnLoad()
    {
        return true;
    }

    protected virtual void OnRender(float deltaTime,
                                    float elapsedSeconds)
    { }

    protected virtual void OnUnload()
    {
        UnbindGlfwCallbacks();
    }

    protected virtual void OnUpdate(float deltaTime,
                                    float elapsedSeconds)
    { }

    protected virtual void OnHandleDebugger(out bool breakOnError)
    {
        breakOnError = false;
    }

    protected virtual void OnWindowResized()
    { }

    protected virtual void OnCharacterInput(char codePoint)
    { }

    protected virtual void OnMouseEnter()
    { }

    protected virtual void OnMouseLeave()
    { }

    protected virtual void OnMouseScrolled(double scrollX,
                                           double scrollY)
    { }

    protected void SetWindowIcon(Image<Rgba32> image)
    {
        unsafe
        {
            if(image.DangerousTryGetSinglePixelMemory(out var memory))
            {
                Glfw.SetWindowIcon(_windowHandle, new Glfw.Image { Width = image.Width, Height = image.Height, PixelPtr = (byte*)memory.Pin().Pointer });
            }
        }
    }

    protected void SetWindowIcon(string fileName)
    {
        if(!File.Exists(fileName))
        {
            _logger.Error("{Category}: Window icon file {FilePath} not found", "App", fileName);

            return;
        }

        using var image = Image.Load<Rgba32>(fileName);
        SetWindowIcon(image);
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

    protected void MaximizeWindow()
    {
        Glfw.MaximizeWindow(_windowHandle);

        _applicationContext.IsWindowMaximized = true;
    }

    protected void RestoreWindow()
    {
        Glfw.RestoreWindow(_windowHandle);

        _applicationContext.IsWindowMaximized = false;
    }

    private void BindGlfwCallbacks()
    {
        _debugProcCallback = DebugCallback;
        _cursorEnterLeaveCallback = OnMouseEnterLeave;
        _cursorPositionCallback = OnMouseMove;
        _keyCallback = OnKey;
        _mouseButtonCallback = OnMouseButton;
        _mouseScrollCallback = OnMouseScroll;
        _framebufferSizeCallback = OnWindowFramebufferSizeChanged;
        _windowSizeCallback = OnWindowSize;
        _windowCharCallback = OnInputCharacter;

        Glfw.SetKeyCallback(_windowHandle, _keyCallback);
        Glfw.SetCursorPositionCallback(_windowHandle, _cursorPositionCallback);
        Glfw.SetCursorEnterCallback(_windowHandle, _cursorEnterLeaveCallback);
        Glfw.SetMouseButtonCallback(_windowHandle, _mouseButtonCallback);
        Glfw.SetScrollCallback(_windowHandle, _mouseScrollCallback);
        Glfw.SetWindowSizeCallback(_windowHandle, _windowSizeCallback);
        Glfw.SetFramebufferSizeCallback(_windowHandle, _framebufferSizeCallback);
        Glfw.SetCharCallback(_windowHandle, _windowCharCallback);
    }

    private void UnbindGlfwCallbacks()
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
        Glfw.SetCharCallback(_windowHandle, null);
    }

    private void OnKey(nint windowHandle,
                       Glfw.Key key,
                       Glfw.Scancode scancode,
                       Glfw.KeyAction action,
                       Glfw.KeyModifiers modifiers)
    {
        if(key != Glfw.Key.Unknown)
        {
            _inputProvider.KeyboardState.SetKeyState(key, action is Glfw.KeyAction.Pressed or Glfw.KeyAction.Repeat);
        }
    }

    private void OnMouseMove(nint windowHandle,
                             double currentCursorX,
                             double currentCursorY)
    {
        if(_isFirstFrame)
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

    private void OnMouseEnterLeave(nint windowHandle,
                                   Glfw.CursorEnterMode cursorEnterMode)
    {
        if(cursorEnterMode == Glfw.CursorEnterMode.Entered)
        {
            OnMouseEnter();
        }
        else
        {
            OnMouseLeave();
        }
    }

    private void OnMouseButton(nint windowHandle,
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

    private void OnMouseScroll(nint windowHandle,
                               double scrollX,
                               double scrollY)
    {
        _inputProvider.MouseState.Scroll += new Vector2((float)scrollX, (float)scrollY);
        OnMouseScrolled(scrollX, scrollY);
    }

    private void OnWindowSize(nint windowHandle,
                              int width,
                              int height)
    {
        _applicationContext.WindowSize = new Int2(width, height);
        OnWindowResized();
    }

    private void OnWindowFramebufferSizeChanged(nint windowHandle,
                                                int width,
                                                int height)
    {
        if(width * height != 0)
        {
            _applicationContext.ResizeWindowFramebuffer(width, height);
        }
    }

    private void OnInputCharacter(nint windowHandle,
                                  uint codePoint)
    {
        OnCharacterInput((char)codePoint);
    }

    private void PrintSystemInformation()
    {
        _logger.Debug("{Category}: Architecture - {@OSArchitecture}", "OS", RuntimeInformation.OSArchitecture);
        _logger.Debug("{Category}: Description - {@OSDescription}", "OS", RuntimeInformation.OSDescription);

        _logger.Debug("{Category}: Framework - {@FrameworkDescription}", "RT", RuntimeInformation.FrameworkDescription);
        _logger.Debug("{Category}: Runtime Identifier - {@RuntimeIdentifier}", "RT", RuntimeInformation.RuntimeIdentifier);
        _logger.Debug("{Category}: Process Architecture - {@ProcessArchitecture}", "RT", RuntimeInformation.ProcessArchitecture);
    }

    private void DebugCallback(GL.DebugSource source,
                               GL.DebugType type,
                               uint id,
                               GL.DebugSeverity severity,
                               int length,
                               nint messagePtr,
                               nint userParam)
    {
        if(type is GL.DebugType.Portability or GL.DebugType.Other or GL.DebugType.PushGroup or GL.DebugType.PopGroup)
        {
            return;
        }

        var message = Marshal.PtrToStringAnsi(messagePtr, length);

        switch(severity)
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

        if(type == GL.DebugType.Error)
        {
            _logger.Error("{@MessageString}", message);

            OnHandleDebugger(out var breakOnError);

            if(breakOnError)
            {
                Debugger.Break();
            }
        }
    }

    private void ErrorCallback(Glfw.ErrorCode errorCode,
                               string errorDescription)
    {
        _logger.Error("{Category}: {ErrorCode} - {ErrorDescription}", "Glfw", errorCode, errorDescription);
    }

    private Task OnMessageCloseWindow(CloseWindowMessage message)
    {
        Close();

        return Task.CompletedTask;
    }

    private Task OnMessageRestoreWindow(RestoreWindowMessage message)
    {
        RestoreWindow();

        return Task.CompletedTask;
    }

    private Task OnMessageMaximizeWindow(MaximizeWindowMessage message)
    {
        MaximizeWindow();

        return Task.CompletedTask;
    }
}
