using EngineKit.Input;

namespace EngineKit;

public interface IInputProvider
{
    MouseState MouseState { get; }
}