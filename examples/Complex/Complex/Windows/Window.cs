using ImGuiNET;

namespace Complex.Windows;

public class Window
{
    protected string? Caption { get; init; }

    protected bool OverwriteStyle { get; init; }

    public void Draw()
    {
        var windowFlags = GetWindowFlags();
        if (OverwriteStyle)
        {
            SetStyleInternal();
        }
        if (ImGui.Begin(Caption, windowFlags))
        {
            DrawInternal();
        }
        ImGui.End();
        if (OverwriteStyle)
        {
            UnsetStyleInternal();
        }
    }

    protected virtual void DrawInternal()
    {
    }

    protected virtual void SetStyleInternal()
    {
    }

    protected virtual void UnsetStyleInternal()
    {
    }

    protected virtual ImGuiWindowFlags GetWindowFlags()
    {
        return ImGuiWindowFlags.None;
    }
}
