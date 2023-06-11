using System;

namespace EngineKit.Native.Glfw;

public static partial class Glfw
{
    public enum ErrorCode
    {
        NoError = 0,
        NotInitialized = 0x00010001,
        NoContext = 0x00010002,
        InvalidEnum = 0x00010003,
        InvalidValue = 0x00010004,
        OutOfMemory = 0x00010005,
        ApiUnavailable = 0x00010006,
        VersionUnavailable = 0x00010007,
        PlatformError = 0x00010008,
        FormatUnavailable = 0x00010009,
        NoWindowContext = 0x0001000A
    }
    
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
        RefreshRate = 0x0002100F
    }

    public enum WindowOpenGLContextHint
    {
        VersionMajor = 0x00022002,
        VersionMinor = 0x00022003,
        ForwardCompatible = 0x00022006,
        DebugContext = 0x00022007,
        Profile = 0x00022008,
    }

    public enum FramebufferInitHint
    {
        RedBits = 0x00021001,
        GreenBits = 0x00021002,
        BlueBits = 0x00021003,
        AlphaBits = 0x00021004,
        DepthBits = 0x00021005,
        StencilBits = 0x00021006,
        Stereo = 0x0002100C,
        Samples = 0x0002100D,
        SrgbCapable = 0x0002100E
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

    public enum InputMode : int
    {
        Cursor = 0x00033001,
        StickyKeys = 0x00033002,
        StickyMouseButtons = 0x00033003,
        LockKeyMods = 0x00033004,
        RawMouseMotion = 0x00033005
    }

    public enum CursorMode : int
    {
        Normal = 0x00034001,
        Hidden = 0x00034002,
        Disabled = 0x00034003,
        Captured = 0x00034004
    }

    public enum Scancode
    {
        KeyBacktick = 96,
        Key0 = 56,
        Key1 = 2,
        Key2 = 3,
        Key3 = 4,
        Key4 = 5,
        Key5 = 6,
        Key6 = 7,
        Key7 = 8,
        Key8 = 9,
        Key9 = 10,
        KeyMinus = 12,
        KeyEqual = 13,
        KeyBackspace = 14,
        KeyTab = 15,
        KeyQ = 16,
        KeyW = 17,
        KeyE = 18,
        KeyR = 19,
        KeyT = 20,
        KeyY = 21,
        KeyU = 22,
        KeyI = 23,
        KeyO = 24,
        KeyP = 25,
        KeyAngledBrackedOpen = 26,
        KeyAngledBrackedClose = 27,
        KeyBackslash = 43,
        KeyCapslock = 58,
        KeyA = 30,
        KeyS = 31,
        KeyD = 32,
        KeyF = 33,
        KeyG = 34,
        KeyH = 35,
        KeyJ = 36,
        KeyK = 37,
        KeyL = 38,
        KeySemicolon = 39,
        KeyApostrophe = 40,
        KeyEnter = 28,
        KeyLeftShift = 42,
        KeyZ = 44,
        KeyX = 45,
        KeyC = 46,
        KeyV = 47,
        KeyB = 48,
        KeyN = 49,
        KeyM = 50,
        KeyComma = 51,
        KeyDot = 52,
        KeySlash = 53,
        KeyRightShift = 54,
        KeyLeftCtrl = 29,
        KeyLeftWin = 347,
        KeyLeftAlt = 56,
        KeySpace = 57,
        KeyRightAlt = 312,
        KeyRightWin = 349,
        KeyRightCtrl = 285,
        KeyEscape = 1,
        KeyF1 = 59,
        KeyF2 = 60,
        KeyF3 = 61,
        KeyF4 = 62,
        KeyF5 = 63,
        KeyF6 = 64,
        KeyF7 = 65,
        KeyF8 = 66,
        KeyF9 = 67,
        KeyF10 = 68,
        KeyF11 = 87,
        KeyF12 = 88,
        KeyPrint = 311,
        KeyScrollLock = 70,
        KeyPause = 69,
        KeyInsert = 338,
        KeyDelete = 339,
        KeyHome = 327,
        KeyEnd = 335,
        KeyPageUp = 329,
        KeyPageDown = 337,
        KeyArrowUp = 328,
        KeyArrowDown = 336,
        KeyArrowLeft = 331,
        KeyArrowRight = 333
    }

    public enum Key : int
    {
        Unknown = -1,
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
        KeyApostrophe = 39,
        KeyB = 66,
        KeyBackslash = 92,
        KeyBackspace = 259,
        KeyBacktick = 96,
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
        KeyLeftShift = 340,
        KeyLeftCtrl = 341,
        KeyLeftAlt = 342,
        KeyRightShift = 344,
        KeyRightCtrl = 345,
        KeyRightAlt = 346,
        KeyArrowLeft = 263,
        KeyArrowRight = 262,
        KeyArrowUp = 265,
        KeyArrowDown = 264,

        KeyPageUp = 266,
        KeyPageDown = 267,
        KeyHome = 268,
        KeyEnd = 269,
        KeyInsert = 260,
        KeyDelete = 261,
        KeyEnter = 257,
        KeyTab = 258,
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