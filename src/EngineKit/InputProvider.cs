using EngineKit.Input;

namespace EngineKit;

internal sealed class InputProvider : IInputProvider
{
    public InputProvider()
    {
        MouseState = new MouseState();
    }

    public MouseState MouseState { get; }
}