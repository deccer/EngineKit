namespace EngineKit;

public class WindowSettings
{
    public int ResolutionWidth { get; set; }

    public int ResolutionHeight { get; set; }

    public float ResolutionScale { get; set; }

    public WindowMode WindowMode { get; set; }

    public bool IsVsyncEnabled { get; set; }

    public string Title { get; set; } = "EngineKit";
}