using ImGuiNET;

namespace Complex.Windows;

public class Window
{
    protected string? Caption { get; init; }
    
    public void Draw()
    {
        var windowFlags = GetWindowFlags();
        if (ImGui.Begin(Caption, windowFlags))
        {
            DrawInternal();
        }
        ImGui.End();
    }

    protected virtual void DrawInternal()
    {
    }

    protected virtual ImGuiWindowFlags GetWindowFlags()
    {
        return ImGuiWindowFlags.None;
    }
}