using System.Collections;
using System.Numerics;
using EngineKit.Native.Glfw;

namespace EngineKit.Input;

public class MouseState
{
    private const int MaxButtons = 16;

    private readonly BitArray _buttons;
    private readonly BitArray _buttonsPrevious;

    public MouseState()
    {
        _buttons = new BitArray(MaxButtons);
        _buttonsPrevious = new BitArray(MaxButtons);
    }

    public void Center()
    {
        DeltaX = 0;
        DeltaY = 0;
    }

    public float X { get; internal set; }

    public float Y { get; internal set; }

    public float DeltaX { get; internal set; }

    public float DeltaY { get; internal set; }

    public float PreviousX { get; internal set; }

    public float PreviousY { get; internal set; }
    
    public Vector2 Scroll { get; internal set; }

    public Vector2 PreviousScroll { get; internal set; }

    public Vector2 ScrollDelta => Scroll - PreviousScroll;

    public override string ToString()
    {
        return $"X: {X} Y: {Y}, DeltaX: {DeltaX} DeltaY: {DeltaY}";
    }

    public bool this[Glfw.MouseButton button]
    {
        get => _buttons[(int)button];
        internal set => _buttons[(int)button] = value;
    }

    public bool IsButtonDown(Glfw.MouseButton button)
    {
        return _buttons[(int)button];
    }
}