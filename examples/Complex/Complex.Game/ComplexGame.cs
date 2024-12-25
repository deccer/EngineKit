using Complex.Engine;
using EngineKit;

namespace Complex.Game;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class ComplexGame : IGame
{
    private readonly IScene _scene;

    public ComplexGame(IScene scene)
    {
        _scene = scene;
    }

    public bool Load()
    {
        return true;
    }

    public void Unload()
    {
    }
}

