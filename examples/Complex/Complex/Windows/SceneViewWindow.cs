using System.Numerics;
using EngineKit;
using EngineKit.UI;
using ImGuiNET;

namespace Complex.Windows;

public class SceneViewWindow : Window
{
    private readonly IApplicationContext _applicationContext;

    private readonly IRenderer _renderer;

    private Vector2 _oldAvailableSize;

    public SceneViewWindow(IRenderer renderer,
                           IApplicationContext applicationContext,
                           IMessageBus messageBus)
    {
        _renderer = renderer;
        _applicationContext = applicationContext;
        Caption = $"{MaterialDesignIcons.ImageFilterHdr} Scene";
        OverwriteStyle = true;
    }

    protected override void DrawInternal()
    {
        var availableSize = ImGui.GetContentRegionAvail();
        if (_oldAvailableSize != availableSize)
        {
            _applicationContext.ResizeSceneView((int)availableSize.X, (int)availableSize.Y);
            _oldAvailableSize = availableSize;
        }

        ImGui.Image((nint)_renderer.GetMainFramebufferDescriptor().ColorAttachments[0].Texture.Id,
                availableSize,
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
