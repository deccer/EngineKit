using System.Collections;
using EngineKit.Native.Glfw;

namespace EngineKit.Input;

public class KeyboardState
{
    private readonly BitArray _keys = new BitArray(512);
    private readonly BitArray _keysPrevious = new BitArray(512);

    private KeyboardState(KeyboardState source)
    {
        _keys = (BitArray)source._keys.Clone();
        _keysPrevious = (BitArray)source._keysPrevious.Clone();
    }

    internal KeyboardState()
    {
    }

    public bool this[Glfw.Key key]
    {
        get => IsKeyDown(key);
        private set => SetKeyState(key, value);
    }

    public bool IsAnyKeyDown
    {
        get
        {
            for (var i = 0; i < _keys.Length; ++i)
            {
                if (_keys[i])
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal void SetKeyState(Glfw.Key key, bool down)
    {
        _keys[(int)key] = down;
    }

    internal void Update()
    {
        _keysPrevious.SetAll(false);
        _keysPrevious.Or(_keys);
    }

    public bool IsKeyDown(Glfw.Key key)
    {
        return _keys[(int)key];
    }

    public bool WasKeyDown(Glfw.Key key)
    {
        return _keysPrevious[(int)key];
    }

    public bool IsKeyPressed(Glfw.Key key)
    {
        return _keys[(int)key] && !_keysPrevious[(int)key];
    }

    public bool IsKeyReleased(Glfw.Key key)
    {
        return !_keys[(int)key] && _keysPrevious[(int)key];
    }

    public KeyboardState GetSnapshot() => new KeyboardState(this);
}