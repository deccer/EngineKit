using System.Runtime.InteropServices;
using System.Xml.Xsl;

namespace EngineKit.Native.Glfw;

public static unsafe partial class Glfw
{
#if PLATFORM_WINDOWS
    private const string GlfwLibrary = "glfw3";
#else
    private const string GlfwLibrary = "glfw";
#endif

    private static readonly delegate* <int> _glfwInit = &GlfwInit;

    private static readonly delegate* <void> _glfwTerminate = &GlfwTerminate;

    private static readonly delegate* <int, int, void> _glfwWindowHint = &GlfwWindowHint;
    
    private static readonly delegate* <int, int, IntPtr, IntPtr, IntPtr, IntPtr> _glfwCreateWindow = &GlfwCreateWindow;

    private static readonly delegate* <IntPtr, void> _glfwDestroyWindow = &GlfwDestroyWindow;

    private static readonly delegate* <IntPtr, int> _glfwWindowShouldClose = &GlfwWindowShouldClose;

    private static readonly delegate* <void> _glfwPollEvents = &GlfwPollEvents;

    private static readonly delegate* <IntPtr, void> _glfwSwapBuffers = &GlfwSwapBuffers;

    private static readonly delegate* <IntPtr, IntPtr> _glfwGetProcAddress = &GlfwGetProcAddress;

    private static readonly delegate* <IntPtr, void> _glfwMakeContextCurrent = &GlfwMakeContextCurrent;

    private static readonly delegate* <IntPtr, int, int, void> _glfwSetWindowPos = &GlfwSetWindowPos;

    private static readonly delegate* <IntPtr, int, int, void> _glfwSetWindowSize = &GlfwSetWindowSize;

    private static readonly delegate* <IntPtr> _glfwGetPrimaryMonitor = &GlfwGetPrimaryMonitor;

    private static readonly delegate* <IntPtr, IntPtr> _glfwGetVideoMode = &GlfwGetVideoMode;

    private static readonly delegate* <IntPtr, IntPtr, void> _glfwSetKeyCallback = &GlfwSetKeyCallback;

    private static readonly delegate* <IntPtr, IntPtr, void> _glfwSetCharCallback = &GlfwSetCharCallback;

    private static readonly delegate* <IntPtr, IntPtr, void> _glfwSetCursorPositionCallback = &GlfwSetCursorPositionCallback;

    private static readonly delegate* <IntPtr, IntPtr, void> _glfwSetCursorEnterCallback = &GlfwSetCursorEnterCallback;

    private static readonly delegate* <IntPtr, IntPtr, void> _glfwSetMouseButtonCallback = &GlfwSetMouseButtonCallback;

    private static readonly delegate* <double> _glfwGetTime = &GlfwGetTime;

    [DllImport(GlfwLibrary, EntryPoint = "glfwInit")]
    private static extern int GlfwInit();

    [DllImport(GlfwLibrary, EntryPoint = "glfwTerminate")]
    private static extern void GlfwTerminate();

    [DllImport(GlfwLibrary, EntryPoint = "glfwWindowHint")]
    private static extern void GlfwWindowHint(
        int hint,
        int value);

    [DllImport(GlfwLibrary, EntryPoint = "glfwCreateWindow")]
    private static extern IntPtr GlfwCreateWindow(
        int width,
        int height,
        IntPtr windowTitle,
        IntPtr monitorHandle,
        IntPtr sharedHandle);

    [DllImport(GlfwLibrary, EntryPoint = "glfwDestroyWindow")]
    private static extern void GlfwDestroyWindow(IntPtr windowHandle);

    [DllImport(GlfwLibrary, EntryPoint = "glfwWindowShouldClose")]
    private static extern int GlfwWindowShouldClose(IntPtr windowHandle);

    [DllImport(GlfwLibrary, EntryPoint = "glfwPollEvents")]
    private static extern void GlfwPollEvents();

    [DllImport(GlfwLibrary, EntryPoint = "glfwSwapBuffers")]
    private static extern void GlfwSwapBuffers(IntPtr windowHandle);

    [DllImport(GlfwLibrary, EntryPoint = "glfwGetProcAddress")]
    private static extern IntPtr GlfwGetProcAddress(IntPtr functionName);

    [DllImport(GlfwLibrary, EntryPoint = "glfwMakeContextCurrent")]
    private static extern void GlfwMakeContextCurrent(IntPtr windowHandle);

    [DllImport(GlfwLibrary, EntryPoint = "glfwSetWindowPos")]
    private static extern void GlfwSetWindowPos(
        IntPtr windowHandle,
        int left,
        int top);

    [DllImport(GlfwLibrary, EntryPoint = "glfwSetWindowSize")]
    private static extern void GlfwSetWindowSize(
        IntPtr windowHandle,
        int width,
        int height);

    [DllImport(GlfwLibrary, EntryPoint = "glfwGetPrimaryMonitor")]
    private static extern IntPtr GlfwGetPrimaryMonitor();

    [DllImport(GlfwLibrary, EntryPoint = "glfwGetVideoMode")]
    private static extern IntPtr GlfwGetVideoMode(IntPtr monitorHandle);

    [DllImport(GlfwLibrary, EntryPoint = "glfwSetKeyCallback")]
    private static extern void GlfwSetKeyCallback(
        IntPtr windowHandle,
        IntPtr keyCallback);

    [DllImport(GlfwLibrary, EntryPoint = "glfwSetCharCallback")]
    private static extern void GlfwSetCharCallback(
        IntPtr windowHandle,
        IntPtr charCallback);

    [DllImport(GlfwLibrary, EntryPoint = "glfwSetCursorPosCallback")]
    private static extern void GlfwSetCursorPositionCallback(
        IntPtr windowHandle,
        IntPtr cursorPositionCallback);

    [DllImport(GlfwLibrary, EntryPoint = "glfwSetCursorEnterCallback")]
    private static extern void GlfwSetCursorEnterCallback(
        IntPtr windowHandle,
        IntPtr cursorEnterCallback);

    [DllImport(GlfwLibrary, EntryPoint = "glfwSetMouseButtonCallback")]
    private static extern void GlfwSetMouseButtonCallback(
        IntPtr windowHandle,
        IntPtr mouseButtonCallback);

    [DllImport(GlfwLibrary, EntryPoint = "glfwGetTime")]
    private static extern double GlfwGetTime();
}