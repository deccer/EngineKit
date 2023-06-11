using System;
using System.Runtime.InteropServices;

namespace EngineKit.Native.Glfw;

public static unsafe partial class Glfw
{
    private static delegate* unmanaged<int> _glfwInitDelegate = &glfwInit;

    private static delegate* unmanaged<void> _glfwTerminateDelegate = &glfwTerminate;

    private static delegate* unmanaged<nint, int> _glfwExtensionSupported = &glfwExtensionSupported;

    private static delegate* unmanaged<int> _glfwRawMouseMotionSupportedDelegate = &glfwRawMouseMotionSupported;

    private static delegate* unmanaged<IntPtr, InputMode, int, void> _glfwSetInputModeDelegate = &glfwSetInputMode;

    private static delegate* unmanaged<int, void> _glfwSwapIntervalDelegate = &glfwSwapInterval;

    private static delegate* unmanaged<IntPtr, double*, double*, void> _glfwGetCursorPosDelegate = &glfwGetCursorPos;

    private static delegate* unmanaged<int, int, void> _glfwWindowHintDelegate = &glfwWindowHint;

    private static delegate* unmanaged<int, int, IntPtr, IntPtr, IntPtr, IntPtr> _glfwCreateWindowDelegate = &glfwCreateWindow;

    private static delegate* unmanaged<IntPtr, void> _glfwDestroyWindowDelegate = &glfwDestroyWindow;

    private static delegate* unmanaged<IntPtr, int> _glfwWindowShouldCloseDelegate = &glfwWindowShouldClose;

    private static delegate* unmanaged<IntPtr, int, void> _glfwSetWindowShouldCloseDelegate = &glfwSetWindowShouldClose;

    private static delegate* unmanaged<void> _glfwPollEventsDelegate = &glfwPollEvents;

    private static delegate* unmanaged<double, void> _glfwWaitEventsTimeoutDelegate = &glfwWaitEventsTimeout;

    private static delegate* unmanaged<IntPtr, void> _glfwSwapBuffersDelegate = &glfwSwapBuffers;

    private static delegate* unmanaged<IntPtr, IntPtr> _glfwGetProcAddressDelegate = &glfwGetProcAddress;

    private static delegate* unmanaged<IntPtr, void> _glfwMakeContextCurrentDelegate = &glfwMakeContextCurrent;

    private static delegate* unmanaged<IntPtr, int, int, void> _glfwSetWindowPosDelegate = &glfwSetWindowPos;

    private static delegate* unmanaged<IntPtr, int, int, void> _glfwSetWindowSizeDelegate = &glfwSetWindowSize;

    private static delegate* unmanaged<IntPtr> _glfwGetPrimaryMonitorDelegate = &glfwGetPrimaryMonitor;

    private static delegate* unmanaged<IntPtr, IntPtr> _glfwGetVideoModeDelegate = &glfwGetVideoMode;

    private static delegate* unmanaged<IntPtr, IntPtr, void> _glfwSetKeyCallbackDelegate = &glfwSetKeyCallback;

    private static delegate* unmanaged<IntPtr, IntPtr, void> _glfwSetCharCallbackDelegate = &glfwSetCharCallback;

    private static delegate* unmanaged<IntPtr, IntPtr, void> _glfwSetCursorPosCallbackDelegate =
        &glfwSetCursorPosCallback;

    private static delegate* unmanaged<IntPtr, IntPtr, void> _glfwSetCursorEnterCallbackDelegate = &glfwSetCursorEnterCallback;

    private static delegate* unmanaged<IntPtr, IntPtr, void> _glfwSetMouseButtonCallbackDelegate = &glfwSetMouseButtonCallback;

    private static delegate* unmanaged<IntPtr, IntPtr, void>
        _glfwSetWindowSizeCallbackDelegate = &glfwSetWindowSizeCallback;

    private static delegate* unmanaged<IntPtr, IntPtr, void> _glfwSetFramebufferSizeCallbackDelegate =
        &glfwSetFramebufferSizeCallback;

    private static delegate* unmanaged<IntPtr, int*, int*, void> _glfwGetFramebufferSizeDelegate = &glfwGetFramebufferSize;

    private static delegate* unmanaged<double> _glfwGetTimeDelegate = &glfwGetTime;

    private static delegate* unmanaged<IntPtr, double, double, void> _glfwSetCursorPosDelegate = &glfwSetCursorPos;

    private static delegate* unmanaged<IntPtr, Key, KeyAction> _glfwGetKeyDelegate = &glfwGetKey;

    private static delegate* unmanaged<IntPtr, void> _glfwSetErrorCallbackDelegate = &glfwSetErrorCallback;

    [UnmanagedCallersOnly]
    private static void glfwSetErrorCallback(IntPtr callback)
    {
        _glfwSetErrorCallbackDelegate = (delegate* unmanaged<IntPtr, void>)NativeLibrary.GetExport(_glfwLibraryHandle, nameof(glfwSetErrorCallback));
        _glfwSetErrorCallbackDelegate(callback);
    }

    [UnmanagedCallersOnly]
    private static int glfwInit()
    {
        _glfwInitDelegate = (delegate* unmanaged<int>)NativeLibrary.GetExport(_glfwLibraryHandle, nameof(glfwInit));
        return _glfwInitDelegate();
    }

    [UnmanagedCallersOnly]
    private static void glfwSetCursorPos(IntPtr windowHandle, double x, double y)
    {
        _glfwSetCursorPosDelegate =
            (delegate* unmanaged<IntPtr, double, double, void>)NativeLibrary.GetExport(_glfwLibraryHandle,
                nameof(glfwSetCursorPos));
        _glfwSetCursorPosDelegate(windowHandle, x, y);
    }

    [UnmanagedCallersOnly]
    private static KeyAction glfwGetKey(IntPtr windowHandle, Key key)
    {
        _glfwGetKeyDelegate = (delegate* unmanaged<IntPtr, Key, KeyAction>)NativeLibrary.GetExport(_glfwLibraryHandle, nameof(glfwGetKey));
        return _glfwGetKeyDelegate(windowHandle, key);
    }

    [UnmanagedCallersOnly]
    private static void glfwTerminate()
    {
        _glfwTerminateDelegate = (delegate* unmanaged<void>)NativeLibrary.GetExport(_glfwLibraryHandle, nameof(glfwTerminate));
        if (_glfwTerminateDelegate != null)
        {
            _glfwTerminateDelegate();
        }
    }

    [UnmanagedCallersOnly]
    private static int glfwExtensionSupported(nint extensionName)
    {
        _glfwExtensionSupported = (delegate* unmanaged<nint, int>)NativeLibrary.GetExport(_glfwLibraryHandle, nameof(glfwExtensionSupported));
        return _glfwExtensionSupported(extensionName);
    }

    [UnmanagedCallersOnly]
    private static int glfwRawMouseMotionSupported()
    {
        _glfwRawMouseMotionSupportedDelegate = (delegate* unmanaged<int>)NativeLibrary.GetExport(_glfwLibraryHandle,
            nameof(glfwRawMouseMotionSupported));
        return _glfwRawMouseMotionSupportedDelegate();
    }

    [UnmanagedCallersOnly]
    private static void glfwSetInputMode(IntPtr windowHandle, InputMode mode, int value)
    {
        _glfwSetInputModeDelegate = (delegate* unmanaged<IntPtr, InputMode, int, void>)NativeLibrary.GetExport(_glfwLibraryHandle, nameof(glfwSetInputMode));
        _glfwSetInputModeDelegate(windowHandle, mode, value);
    }

    [UnmanagedCallersOnly]
    private static void glfwGetCursorPos(
        IntPtr windowHandle,
        double* x,
        double* y)
    {
        _glfwGetCursorPosDelegate = (delegate* unmanaged<IntPtr, double*, double*, void>)NativeLibrary.GetExport(_glfwLibraryHandle, nameof(glfwGetCursorPos));
        _glfwGetCursorPosDelegate(windowHandle, x, y);
    }

    [UnmanagedCallersOnly]
    private static void glfwWindowHint(
        int hint,
        int value)
    {
        _glfwWindowHintDelegate = (delegate* unmanaged<int, int, void>)NativeLibrary.GetExport(_glfwLibraryHandle, nameof(glfwWindowHint));
        _glfwWindowHintDelegate(hint, value);
    }

    [UnmanagedCallersOnly]
    private static IntPtr glfwCreateWindow(
        int width,
        int height,
        IntPtr windowTitle,
        IntPtr monitorHandle,
        IntPtr sharedHandle)
    {
        _glfwCreateWindowDelegate = (delegate* unmanaged<int, int, IntPtr, IntPtr, IntPtr, IntPtr>)NativeLibrary.GetExport(_glfwLibraryHandle, nameof(glfwCreateWindow));
        return _glfwCreateWindowDelegate(width, height, windowTitle, monitorHandle, sharedHandle);
    }

    [UnmanagedCallersOnly]
    private static void glfwDestroyWindow(IntPtr windowHandle)
    {
        _glfwDestroyWindowDelegate = (delegate* unmanaged<IntPtr, void>)NativeLibrary.GetExport(_glfwLibraryHandle, nameof(glfwDestroyWindow));
        _glfwDestroyWindowDelegate(windowHandle);
    }

    [UnmanagedCallersOnly]
    private static int glfwWindowShouldClose(IntPtr windowHandle)
    {
        _glfwWindowShouldCloseDelegate = (delegate* unmanaged<IntPtr, int>)NativeLibrary.GetExport(_glfwLibraryHandle, nameof(glfwWindowShouldClose));
        return _glfwWindowShouldCloseDelegate(windowHandle);
    }

    [UnmanagedCallersOnly]
    private static void glfwSetWindowShouldClose(IntPtr windowHandle, int closeFlag)
    {
        _glfwSetWindowShouldCloseDelegate = (delegate* unmanaged<IntPtr, int, void>)NativeLibrary.GetExport(_glfwLibraryHandle,
            nameof(glfwSetWindowShouldClose));
        _glfwSetWindowShouldCloseDelegate(windowHandle, closeFlag);
    }

    [UnmanagedCallersOnly]
    private static void glfwPollEvents()
    {
        _glfwPollEventsDelegate = (delegate* unmanaged<void>)NativeLibrary.GetExport(_glfwLibraryHandle, nameof(glfwPollEvents));
        _glfwPollEventsDelegate();
    }

    [UnmanagedCallersOnly]
    private static void glfwWaitEventsTimeout(double timeout)
    {
        _glfwWaitEventsTimeoutDelegate = (delegate* unmanaged<double, void>)NativeLibrary.GetExport(_glfwLibraryHandle, nameof(glfwWaitEventsTimeout));
        _glfwWaitEventsTimeoutDelegate(timeout);
    }

    [UnmanagedCallersOnly]
    private static void glfwSwapBuffers(IntPtr windowHandle)
    {
        _glfwSwapBuffersDelegate = (delegate* unmanaged<IntPtr, void>)NativeLibrary.GetExport(_glfwLibraryHandle, nameof(glfwSwapBuffers));
        _glfwSwapBuffersDelegate(windowHandle);
    }

    [UnmanagedCallersOnly]
    private static IntPtr glfwGetProcAddress(IntPtr functionName)
    {
        _glfwGetProcAddressDelegate =
            (delegate* unmanaged<IntPtr, IntPtr>)NativeLibrary.GetExport(_glfwLibraryHandle,
                nameof(glfwGetProcAddress));
        return _glfwGetProcAddressDelegate(functionName);
    }

    [UnmanagedCallersOnly]
    private static void glfwMakeContextCurrent(IntPtr windowHandle)
    {
        _glfwMakeContextCurrentDelegate = (delegate* unmanaged<IntPtr, void>)NativeLibrary.GetExport(_glfwLibraryHandle, nameof(glfwMakeContextCurrent));
        _glfwMakeContextCurrentDelegate(windowHandle);
    }

    [UnmanagedCallersOnly]
    private static void glfwSwapInterval(int interval)
    {
        _glfwSwapIntervalDelegate = (delegate* unmanaged<int, void>)NativeLibrary.GetExport(_glfwLibraryHandle, nameof(glfwSwapInterval));
        _glfwSwapIntervalDelegate(interval);
    }

    [UnmanagedCallersOnly]
    private static void glfwSetWindowPos(
        IntPtr windowHandle,
        int left,
        int top)
    {
        _glfwSetWindowPosDelegate = (delegate* unmanaged<IntPtr, int, int, void>)NativeLibrary.GetExport(_glfwLibraryHandle, nameof(glfwSetWindowPos));
        _glfwSetWindowPosDelegate(windowHandle, left, top);
    }

    [UnmanagedCallersOnly]
    private static void glfwSetWindowSize(
        IntPtr windowHandle,
        int width,
        int height)
    {
        _glfwSetWindowSizeDelegate =
            (delegate* unmanaged<IntPtr, int, int, void>)NativeLibrary.GetExport(_glfwLibraryHandle,
                nameof(glfwSetWindowSize));
        _glfwSetWindowSizeDelegate(windowHandle, width, height);
    }

    [UnmanagedCallersOnly]
    private static IntPtr glfwGetPrimaryMonitor()
    {
        _glfwGetPrimaryMonitorDelegate =
            (delegate* unmanaged<IntPtr>)NativeLibrary.GetExport(_glfwLibraryHandle, nameof(glfwGetPrimaryMonitor));
        return _glfwGetPrimaryMonitorDelegate();
    }

    [UnmanagedCallersOnly]
    private static IntPtr glfwGetVideoMode(IntPtr monitorHandle)
    {
        _glfwGetVideoModeDelegate =
            (delegate* unmanaged<IntPtr, IntPtr>)NativeLibrary.GetExport(_glfwLibraryHandle, nameof(glfwGetVideoMode));
        return _glfwGetVideoModeDelegate(monitorHandle);
    }

    [UnmanagedCallersOnly]
    private static void glfwSetKeyCallback(
        IntPtr windowHandle,
        IntPtr keyCallback)
    {
        _glfwSetKeyCallbackDelegate =
            (delegate* unmanaged<IntPtr, IntPtr, void>)NativeLibrary.GetExport(_glfwLibraryHandle,
                nameof(glfwSetKeyCallback));
        _glfwSetKeyCallbackDelegate(windowHandle, keyCallback);
    }

    [UnmanagedCallersOnly]
    private static void glfwSetCharCallback(
        IntPtr windowHandle,
        IntPtr charCallback)
    {
        _glfwSetCharCallbackDelegate =
            (delegate* unmanaged<IntPtr, IntPtr, void>)NativeLibrary.GetExport(_glfwLibraryHandle,
                nameof(glfwSetCharCallback));
        _glfwSetCharCallbackDelegate(windowHandle, charCallback);
    }

    [UnmanagedCallersOnly]
    private static void glfwSetCursorPosCallback(
        IntPtr windowHandle,
        IntPtr cursorPositionCallback)
    {
        _glfwSetCursorPosCallbackDelegate = (delegate* unmanaged<IntPtr, IntPtr, void>)NativeLibrary.GetExport(
            _glfwLibraryHandle,
            nameof(glfwSetCursorPosCallback));
        _glfwSetCursorPosCallbackDelegate(windowHandle, cursorPositionCallback);
    }

    [UnmanagedCallersOnly]
    private static void glfwSetCursorEnterCallback(
        IntPtr windowHandle,
        IntPtr cursorEnterCallback)
    {
        _glfwSetCursorEnterCallbackDelegate = (delegate* unmanaged<IntPtr, IntPtr, void>)NativeLibrary.GetExport(_glfwLibraryHandle,
            nameof(glfwSetCursorEnterCallback));
        _glfwSetCursorEnterCallbackDelegate(windowHandle, cursorEnterCallback);
    }

    [UnmanagedCallersOnly]
    private static void glfwSetMouseButtonCallback(
        IntPtr windowHandle,
        IntPtr mouseButtonCallback)
    {
        _glfwSetMouseButtonCallbackDelegate = (delegate* unmanaged<IntPtr, IntPtr, void>)NativeLibrary.GetExport(_glfwLibraryHandle,
            nameof(glfwSetMouseButtonCallback));
        _glfwSetMouseButtonCallbackDelegate(windowHandle, mouseButtonCallback);
    }

    [UnmanagedCallersOnly]
    private static void glfwSetWindowSizeCallback(
        IntPtr windowHandle,
        IntPtr windowSizeCallback)
    {
        _glfwSetWindowSizeCallbackDelegate = (delegate* unmanaged<IntPtr, IntPtr, void>)NativeLibrary.GetExport(_glfwLibraryHandle,
            nameof(glfwSetWindowSizeCallback));
        _glfwSetWindowSizeCallbackDelegate(windowHandle, windowSizeCallback);
    }

    [UnmanagedCallersOnly]
    private static void glfwSetFramebufferSizeCallback(
        IntPtr windowHandle,
        IntPtr framebufferSizeCallback)
    {
        _glfwSetFramebufferSizeCallbackDelegate =
            (delegate* unmanaged<IntPtr, IntPtr, void>)NativeLibrary.GetExport(_glfwLibraryHandle,
                nameof(glfwSetFramebufferSizeCallback));
        _glfwSetFramebufferSizeCallbackDelegate(windowHandle, framebufferSizeCallback);
    }

    [UnmanagedCallersOnly]
    private static void glfwGetFramebufferSize(IntPtr windowHandle, int* width, int* height)
    {
        _glfwGetFramebufferSizeDelegate =
            (delegate* unmanaged<IntPtr, int*, int*, void>)NativeLibrary.GetExport(_glfwLibraryHandle,
                nameof(glfwGetFramebufferSize));
        _glfwGetFramebufferSizeDelegate(windowHandle, width, height);
    }

    [UnmanagedCallersOnly]
    private static double glfwGetTime()
    {
        _glfwGetTimeDelegate =
            (delegate* unmanaged<double>)NativeLibrary.GetExport(_glfwLibraryHandle, nameof(glfwGetTime));
        return _glfwGetTimeDelegate();
    }
}