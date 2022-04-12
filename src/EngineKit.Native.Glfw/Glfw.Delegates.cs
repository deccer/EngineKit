namespace EngineKit.Native.Glfw;

public static partial class Glfw
{
    public delegate void KeyCallback(
        IntPtr windowHandle,
        Key key,
        int scanCode,
        KeyAction action,
        KeyModifiers modifiers);

    public delegate void CharCallback(
        IntPtr windowHandle,
        uint codePoint);

    public delegate void CursorPositionCallback(
        IntPtr windowHandle,
        double x,
        double y);

    public delegate void CursorEnterCallback(
        IntPtr windowHandle,
        CursorEnterMode cursorEnterMode);

    public delegate void MouseButtonCallback(
        IntPtr windowHandle,
        MouseButton mouseButton,
        KeyAction action,
        KeyModifiers modifiers);
}