using EngineKit.Native.Glfw;

public static class Program
{
    public static void Main()
    {
        if (!Glfw.Init())
        {
            return;
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

        var windowHandle = Glfw.CreateWindow(windowWidth, windowHeight, "Hello", IntPtr.Zero, IntPtr.Zero);
        if (windowHandle == IntPtr.Zero)
        {
            return;
        }

        Glfw.SetWindowPos(windowHandle, screenWidth / 2 - windowWidth / 2, screenHeight / 2 - windowHeight / 2);

        Glfw.SetKeyCallback(windowHandle, OnKey);
        Glfw.SetCursorPositionCallback(windowHandle, OnMousePosition);
        Glfw.SetCursorEnterCallback(windowHandle, OnMouseEnter);
        Glfw.SetMouseButtonCallback(windowHandle, OnMouseButton);

        Glfw.MakeContextCurrent(windowHandle);

        while (!Glfw.ShouldWindowClose(windowHandle))
        {
            Glfw.PollEvents();

            Glfw.SwapBuffers(windowHandle);
        }

        Glfw.SetKeyCallback(windowHandle, null);
        Glfw.SetCursorEnterCallback(windowHandle, null);
        Glfw.SetCursorPositionCallback(windowHandle, null);
        Glfw.SetMouseButtonCallback(windowHandle, null);
        Glfw.DestroyWindow(windowHandle);
        Glfw.Terminate();
    }
    
    private static void OnKey(
        IntPtr windowHandle,
        Glfw.Key key,
        int scanCode,
        Glfw.KeyAction action,
        Glfw.KeyModifiers modifiers)
    {
        Console.WriteLine($"key: {key} scancode: {scanCode} action: {action} modifiers: {modifiers}");
    }

    private static void OnMousePosition(
        IntPtr windowHandle,
        double x,
        double y)
    {
        Console.WriteLine($"x: {x} y: {y}");
    }

    private static void OnMouseEnter(
        IntPtr windowHandle,
        Glfw.CursorEnterMode cursorEnterMode)
    {
        Console.WriteLine($"mode: {cursorEnterMode}");
    }

    private static void OnMouseButton(
        IntPtr windowHandle,
        Glfw.MouseButton mouseButton,
        Glfw.KeyAction action,
        Glfw.KeyModifiers modifiers)
    {
        Console.WriteLine($"button: {mouseButton} action: {action} modifiers: {modifiers}");
    }
}
