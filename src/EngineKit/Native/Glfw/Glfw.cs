using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace EngineKit.Native.Glfw;

public static unsafe partial class Glfw
{
    private static IntPtr _glfwLibraryHandle;
    private static bool _glfwLibraryLoaded;

    public const int True = 1;
    public const int False = 0;
    
    public const int CursorNormal = 0x00034001;
    public const int CursorHidden = 0x00034002;
    public const int CursorDisabled = 0x00034003;

    public static bool Init()
    {
        if (_glfwLibraryLoaded)
        {
            return _glfwInitDelegate() == True;
        }
        
        Console.WriteLine(RuntimeInformation.RuntimeIdentifier);

        var libraryName = "./runtimes/win-x64/native/glfw3.dll";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            libraryName = RuntimeInformation.RuntimeIdentifier.Contains("ubuntu")
                ? File.Exists("/usr/lib/x86_64-linux-gnu/libglfw.so")
                    ? "/usr/lib/x86_64-linux-gnu/libglfw.so"
                    : "libglfw.so.3"
                : "libglfw.so.3";
        }

        if (!NativeLibrary.TryLoad(libraryName, out _glfwLibraryHandle))
        {
            Debug.WriteLine($"GLFW: Unable to load {libraryName}");
            return false;
        }

        _glfwLibraryLoaded = true;

        return _glfwInitDelegate() == True;
    }

    public static void Terminate()
    {
        _glfwTerminateDelegate();
        if (_glfwLibraryLoaded)
        {
            NativeLibrary.Free(_glfwLibraryHandle);
        }
    }

    public static bool IsRawMouseMotionSupported()
    {
        return _glfwRawMouseMotionSupportedDelegate() == True;
    }

    public static bool IsExtensionSupported(string extensionName)
    {
        var extensionNamePtr = Marshal.StringToHGlobalAnsi(extensionName);
        var result = _glfwExtensionSupported(extensionNamePtr);
        Marshal.FreeHGlobal(extensionNamePtr);

        return result == True;
    }

    public static KeyAction GetKey(IntPtr windowHandle, Key key)
    {
        return _glfwGetKeyDelegate(windowHandle, key);
    }

    public static bool GetKeyPressed(IntPtr windowHandle, Key key)
    {
        var keyAction = _glfwGetKeyDelegate(windowHandle, key);
        return keyAction is KeyAction.Pressed or KeyAction.Repeat;
    }

    public static void SetInputMode(
        IntPtr windowHandle,
        InputMode inputMode,
        int value)
    {
        _glfwSetInputModeDelegate(windowHandle, inputMode, value);
    }

    public static void GetCursorPos(
        IntPtr windowHandle,
        out int x,
        out int y)
    {
        double cursorPosX;
        double cursorPosY;
        _glfwGetCursorPosDelegate(windowHandle, &cursorPosX, &cursorPosY);
        x = (int)cursorPosX;
        y = (int)cursorPosY;
    }

    public static void SetCursorPos(
        IntPtr windowHandle,
        float x,
        float y)
    {
        _glfwSetCursorPosDelegate(windowHandle, x, y);
    }

    public static void WindowHint(
        WindowInitHint windowInitHint,
        bool value)
    {
        _glfwWindowHintDelegate((int)windowInitHint, value ? True : False);
    }

    public static void WindowHint(
        WindowInitHint windowInitHint,
        int value)
    {
        _glfwWindowHintDelegate((int)windowInitHint, value);
    }

    public static void WindowHint(
        WindowInitHint windowInitHint,
        ClientApi clientApi)
    {
        _glfwWindowHintDelegate((int)windowInitHint, (int)clientApi);
    }

    public static void WindowHint(
        FramebufferInitHint framebufferInitHint,
        int value)
    {
        _glfwWindowHintDelegate((int)framebufferInitHint, value);
    }
    
    public static void WindowHint(
        FramebufferInitHint framebufferInitHint,
        bool value)
    {
        _glfwWindowHintDelegate((int)framebufferInitHint, value ? True : False);
    }

    public static void WindowHint(
        WindowOpenGLContextHint openglContextHint,
        int value)
    {
        _glfwWindowHintDelegate((int)openglContextHint, value);
    }

    public static void WindowHint(
        WindowOpenGLContextHint openglContextHint,
        bool value)
    {
        _glfwWindowHintDelegate((int)openglContextHint, value ? True : False);
    }

    public static void WindowHint(
        WindowOpenGLContextHint openglContextHint,
        OpenGLProfile openGlProfile)
    {
        _glfwWindowHintDelegate((int)openglContextHint, (int)openGlProfile);
    }

    public static IntPtr CreateWindow(
        int width,
        int height,
        string title,
        IntPtr monitorHandle,
        IntPtr sharedHandle)
    {
        var titlePtr = Marshal.StringToHGlobalAnsi(title);
        var windowHandle = _glfwCreateWindowDelegate(
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
        _glfwDestroyWindowDelegate(windowHandle);
    }

    public static bool ShouldWindowClose(IntPtr windowHandle)
    {
        return _glfwWindowShouldCloseDelegate(windowHandle) == True;
    }

    public static void SetWindowShouldClose(
        IntPtr windowHandle,
        int closeFlag)
    {
        _glfwSetWindowShouldCloseDelegate(windowHandle, closeFlag);
    }

    public static void PollEvents()
    {
        _glfwPollEventsDelegate();
    }

    public static void WaitEventsTimeout(double timeout)
    {
        _glfwWaitEventsTimeoutDelegate(timeout);
    }

    public static void SwapBuffers(IntPtr windowHandle)
    {
        _glfwSwapBuffersDelegate(windowHandle);
    }

    public static IntPtr GetProcAddress(string functionName)
    {
        var functionNamePtr = Marshal.StringToHGlobalAnsi(functionName);
        var functionAddress = _glfwGetProcAddressDelegate(functionNamePtr);
        Marshal.FreeHGlobal(functionNamePtr);

        return functionAddress;
    }

    public static void MakeContextCurrent(IntPtr windowHandle)
    {
        _glfwMakeContextCurrentDelegate(windowHandle);
    }

    public static void SwapInterval(int interval)
    {
        _glfwSwapIntervalDelegate(interval);
    }

    public static void SetWindowPos(
        IntPtr windowHandle,
        int left,
        int top)
    {
        _glfwSetWindowPosDelegate(windowHandle, left, top);
    }

    public static void SetWindowSize(
        IntPtr windowHandle,
        int width,
        int height)
    {
        _glfwSetWindowSizeDelegate(windowHandle, width, height);
    }

    public static IntPtr GetPrimaryMonitor()
    {
        return _glfwGetPrimaryMonitorDelegate();
    }

    public static VideoMode GetVideoMode(IntPtr monitorHandle)
    {
        var videoMode = _glfwGetVideoModeDelegate(monitorHandle);
        return Marshal.PtrToStructure<VideoMode>(videoMode);
    }

    public static void SetKeyCallback(
        IntPtr windowHandle,
        KeyCallback? keyCallback)
    {
        var keyCallbackPtr = keyCallback == null
            ? IntPtr.Zero
            : Marshal.GetFunctionPointerForDelegate(keyCallback);
        _glfwSetKeyCallbackDelegate(windowHandle, keyCallbackPtr);
    }

    public static void SetCharCallback(
        IntPtr windowHandle,
        CharCallback? charCallback)
    {
        var charCallbackPtr = charCallback == null
            ? IntPtr.Zero
            : Marshal.GetFunctionPointerForDelegate(charCallback);
        _glfwSetCharCallbackDelegate(windowHandle, charCallbackPtr);
    }

    public static void SetCursorPositionCallback(
        IntPtr windowHandle,
        CursorPositionCallback? cursorPositionCallback)
    {
        var cursorPositionCallbackPtr = cursorPositionCallback == null
            ? IntPtr.Zero
            : Marshal.GetFunctionPointerForDelegate(cursorPositionCallback);
        _glfwSetCursorPosCallbackDelegate(windowHandle, cursorPositionCallbackPtr);
    }

    public static void SetCursorEnterCallback(
        IntPtr windowHandle,
        CursorEnterCallback? cursorEnterCallback)
    {
        var cursorEnterCallbackPtr = cursorEnterCallback == null
            ? IntPtr.Zero
            : Marshal.GetFunctionPointerForDelegate(cursorEnterCallback);
        _glfwSetCursorEnterCallbackDelegate(windowHandle, cursorEnterCallbackPtr);
    }

    public static void SetMouseButtonCallback(
        IntPtr windowHandle,
        MouseButtonCallback? mouseButtonCallback)
    {
        var mouseButtonCallbackPtr = mouseButtonCallback == null
            ? IntPtr.Zero
            : Marshal.GetFunctionPointerForDelegate(mouseButtonCallback);
        _glfwSetMouseButtonCallbackDelegate(windowHandle, mouseButtonCallbackPtr);
    }

    public static void SetWindowSizeCallback(
        IntPtr windowHandle,
        WindowSizeCallback? windowSizeCallback)
    {
        var sizeWindowCallbackPtr = windowSizeCallback == null
            ? IntPtr.Zero
            : Marshal.GetFunctionPointerForDelegate(windowSizeCallback);
        _glfwSetWindowSizeCallbackDelegate(windowHandle, sizeWindowCallbackPtr);
    }

    public static void SetFramebufferSizeCallback(
        IntPtr windowHandle,
        FramebufferSizeCallback? framebufferSizeCallback)
    {
        var framebufferSizeCallbackPtr = framebufferSizeCallback == null
            ? IntPtr.Zero
            : Marshal.GetFunctionPointerForDelegate(framebufferSizeCallback);
        _glfwSetFramebufferSizeCallbackDelegate(windowHandle, framebufferSizeCallbackPtr);
    }

    public static void SetScrollCallback(IntPtr windowHandle, ScrollCallback? scrollCallback)
    {
        var scrollCallbackPtr = scrollCallback == null
            ? IntPtr.Zero
            : Marshal.GetFunctionPointerForDelegate(scrollCallback);
        _glfwSetScrollCallbackDelegate(windowHandle, scrollCallbackPtr);
    }

    public static void SetErrorCallback(ErrorCallback? errorCallback)
    {
        var errorCallbackPtr = errorCallback == null
            ? IntPtr.Zero
            : Marshal.GetFunctionPointerForDelegate(errorCallback);
        _glfwSetErrorCallbackDelegate(errorCallbackPtr);
    }

    public static float GetTime()
    {
        return (float)_glfwGetTimeDelegate();
    }


    public static void GetFramebufferSize(
        IntPtr windowHandle,
        out int framebufferWidth,
        out int framebufferHeight)
    {
        int width;
        int height;
        _glfwGetFramebufferSizeDelegate(windowHandle, &width, &height);
        framebufferWidth = width;
        framebufferHeight = height;
    }
}