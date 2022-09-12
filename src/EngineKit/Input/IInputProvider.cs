namespace EngineKit.Input;

public interface IInputProvider
{
    KeyboardState KeyboardState { get; }

    MouseState MouseState { get; }
}