using System.Numerics;
using EngineKit;
using EngineKit.Mathematics;
using EngineKit.UI;
using ImGuiNET;

namespace Complex.Windows;

public class SceneViewWindow : Window
{
    private readonly IRenderer _renderer;
    private readonly IApplicationContext _applicationContext;

    public SceneViewWindow(IRenderer renderer, IApplicationContext applicationContext)
    {
        _renderer = renderer;
        _applicationContext = applicationContext;
        Caption = $"{MaterialDesignIcons.ImageFilterHdr} Scene";
        OverwriteStyle = true;
    }

    protected override void DrawInternal()
    {
        var availableSize = ImGui.GetContentRegionAvail();

        _applicationContext.FramebufferSize = new Int2((int)availableSize.X, (int)availableSize.Y);
        
        ImGui.Image(
            (nint)_renderer.GetMainFrameDescriptor().ColorAttachments[0].Texture.Id, availableSize,
            Vector2.UnitY, 
            Vector2.UnitX);
    }
    
    protected override void SetStyleInternal()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
    }

    protected override void UnsetStyleInternal()
    {
        ImGui.PopStyleVar();
    }
}