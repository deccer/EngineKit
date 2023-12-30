using ImGuiNET;

namespace Complex.Windows;

public class Window
{
    public string Caption { get; set; }
    
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