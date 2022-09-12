namespace EngineKit.Input;

internal sealed class InputProvider : IInputProvider
{
    public InputProvider()
    {
        KeyboardState = new KeyboardState();
        MouseState = new MouseState();
    }

    public KeyboardState KeyboardState { get; }

    public MouseState MouseState { get; }
}