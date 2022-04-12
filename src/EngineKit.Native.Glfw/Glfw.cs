using System.Runtime.InteropServices;

namespace EngineKit.Native.Glfw;

public static unsafe partial class Glfw
{
    public static bool Init()
    {
        return _glfwInit() == 1;
    }

    public static void Terminate()
    {
        _glfwTerminate();
    }

    public static void WindowHint(
        WindowInitHint windowInitHint,
        bool value)
    {
        _glfwWindowHint((int)windowInitHint, value ? 1 : 0);
    }

    public static void WindowHint(
        WindowInitHint windowInitHint,
        int value)
    {
        _glfwWindowHint((int)windowInitHint, value);
    }

    public static void WindowHint(
        WindowInitHint windowInitHint,
        ClientApi clientApi)
    {
        _glfwWindowHint((int)windowInitHint, (int)clientApi);
    }
    
    public static void WindowHint(
        WindowOpenGLContextHint openglContextHint,
        int value)
    {
        _glfwWindowHint((int)openglContextHint, value);
    }
    
    public static void WindowHint(
        WindowOpenGLContextHint openglContextHint,
        bool value)
    {
        _glfwWindowHint((int)openglContextHint, value ? 1 : 0);
    }

    public static void WindowHint(
        WindowOpenGLContextHint openglContextHint,
        OpenGLProfile openGlProfile)
    {
        _glfwWindowHint((int)openglContextHint, (int)openGlProfile);
    }

    public static IntPtr CreateWindow(
        int width,
        int height,
        string title,
        IntPtr monitorHandle,
        IntPtr sharedHandle)
    {
        var titlePtr = Marshal.StringToHGlobalAnsi(title);
        var windowHandle = _glfwCreateWindow(
            width,
            height,
            titlePtr,
            monitorHandle,
            sharedHandle);
        Marshal.FreeHGlobal(titlePtr);
        return windowHandle;
    }

    public static void DestroyWindow(IntPtr windowHandle)
    {
        _glfwDestroyWindow(windowHandle);
    }

    public static bool ShouldWindowClose(IntPtr windowHandle)
    {
        return _glfwWindowShouldClose(windowHandle) == 1;
    }

    public static void PollEvents()
    {
        _glfwPollEvents();
    }

    public static void SwapBuffers(IntPtr windowHandle)
    {
        _glfwSwapBuffers(windowHandle);
    }

    public static IntPtr GetProcAddress(string functionName)
    {
        var functionNamePtr = Marshal.StringToHGlobalAnsi(functionName);
        var functionAddress = _glfwGetProcAddress(functionNamePtr);
        Marshal.FreeHGlobal(functionNamePtr);
        return functionAddress;
    }

    public static void MakeContextCurrent(IntPtr windowHandle)
    {
        _glfwMakeContextCurrent(windowHandle);
    }

    public static void SetWindowPos(
        IntPtr windowHandle,
        int left,
        int top)
    {
        _glfwSetWindowPos(windowHandle, left, top);
    }

    public static void SetWindowSize(
        IntPtr windowHandle,
        int width,
        int height)
    {
        _glfwSetWindowSize(windowHandle, width, height);
    }

    public static IntPtr GetPrimaryMonitor()
    {
        return _glfwGetPrimaryMonitor();
    }

    public static VideoMode GetVideoMode(IntPtr monitorHandle)
    {
        var videoMode = _glfwGetVideoMode(monitorHandle);
        return Marshal.PtrToStructure<VideoMode>(videoMode);
    }

    public static void SetKeyCallback(
        IntPtr windowHandle,
        KeyCallback? keyCallback)
    {
        var keyCallbackPtr = keyCallback == null
            ? IntPtr.Zero
            : Marshal.GetFunctionPointerForDelegate(keyCallback);
        _glfwSetKeyCallback(windowHandle, keyCallbackPtr);
    }

    public static void SetCharCallback(
        IntPtr windowHandle,
        CharCallback? charCallback)
    {
        var charCallbackPtr = charCallback == null
            ? IntPtr.Zero
            : Marshal.GetFunctionPointerForDelegate(charCallback);
        _glfwSetCharCallback(windowHandle, charCallbackPtr);
    }

    public static void SetCursorPositionCallback(
        IntPtr windowHandle,
        CursorPositionCallback? cursorPositionCallback)
    {
        var cursorPositionCallbackPtr = cursorPositionCallback == null
            ? IntPtr.Zero
            : Marshal.GetFunctionPointerForDelegate(cursorPositionCallback);
        _glfwSetCursorPositionCallback(windowHandle, cursorPositionCallbackPtr);
    }

    public static void SetCursorEnterCallback(
        IntPtr windowHandle,
        CursorEnterCallback? cursorEnterCallback)
    {
        var cursorEnterCallbackPtr = cursorEnterCallback == null
            ? IntPtr.Zero
            : Marshal.GetFunctionPointerForDelegate(cursorEnterCallback);
        _glfwSetCursorEnterCallback(windowHandle, cursorEnterCallbackPtr);
    }

    public static void SetMouseButtonCallback(
        IntPtr windowHandle,
        MouseButtonCallback? mouseButtonCallback)
    {
        var mouseButtonCallbackPtr = mouseButtonCallback == null
            ? IntPtr.Zero
            : Marshal.GetFunctionPointerForDelegate(mouseButtonCallback);
        _glfwSetMouseButtonCallback(windowHandle, mouseButtonCallbackPtr);
    }

    public static float GetTime()
    {
        return (float)_glfwGetTime();
    }
}