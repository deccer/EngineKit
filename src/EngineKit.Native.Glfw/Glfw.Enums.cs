namespace EngineKit.Native.Glfw;

public static unsafe partial class Glfw
{
    public enum WindowInitHint
    {
        IsResizeable = 0x00020003,
        IsVisible = 0x00020004,
        IsDecorated = 0x00020005,
        IsFocused = 0x00020001,
        AutoIconify = 0x00020006,
        IsFloating = 0x00020007,
        IsMaximized = 0x00020008,
        IsIconified = 0x00020002,
        CenterCursor = 0x00020009,
        TransparentFramebuffer = 0x0002000A,
        FocusOnShow = 0x0002000C,
        ScaleToMonitor = 0x0002200C,
        DoubleBuffer = 0x00021010,
        ClientApi = 0x00022001,
    }

    public enum WindowOpenGLContextHint
    {
        VersionMajor = 0x00022002,
        VersionMinor = 0x00022003,
        ForwardCompatible = 0x00022006,
        DebugContext = 0x00022007,
        Profile = 0x00022008,
    }

    public enum OpenGLProfile
    {
        Any = 0,
        Core = 0x00032001,
        Compatible = 0x00032002
    }

    public enum ClientApi : int
    {
        None = 0,
        OpenGL = 0x00030001,
        OpenGLES = 0x00030002
    }

    public enum Key
    {
        Key0 = 48,
        Key1 = 49,
        Key2 = 50,
        Key3 = 51,
        Key4 = 52,
        Key5 = 53,
        Key6 = 54,
        Key7 = 55,
        Key8 = 56,
        Key9 = 57,
        KeyA = 65,
        KeyB = 66,
        KeyC = 67,
        KeyD = 68,
        KeyE = 69,
        KeyF = 70,
        KeyG = 71,
        KeyH = 72,
        KeyI = 73,
        KeyJ = 74,
        KeyK = 75,
        KeyL = 76,
        KeyM = 77,
        KeyN = 78,
        KeyO = 79,
        KeyP = 80,
        KeyQ = 81,
        KeyR = 82,
        KeyS = 83,
        KeyT = 84,
        KeyU = 85,
        KeyV = 86,
        KeyW = 87,
        KeyX = 88,
        KeyY = 89,
        KeyZ = 90,
        KeyEscape = 256,
        KeyF1 = 290,
        KeyF2 = 291,
        KeyF3 = 292,
        KeyF4 = 293,
        KeyF5 = 294,
        KeyF6 = 295,
        KeyF7 = 296,
        KeyF8 = 297,
        KeyF9 = 298,
        KeyF10 = 299,
        KeyF11 = 300,
        KeyF12 = 301,
        KeyControlLeft = 341,
        KeyControlRight = 345
    }

    public enum KeyAction
    {
        Released,
        Pressed,
        Repeat
    }

    [Flags]
    public enum KeyModifiers
    {
        None = 0,
        Shift = 1,
        Control = 2,
        Alt = 4
    }

    public enum CursorEnterMode
    {
        Left = 0,
        Entered = 1
    }

    public enum MouseButton
    {
        ButtonLeft,
        ButtonRight,
        ButtonMiddle,
        ButtonFour,
        ButtonFive
    }
}