using System.Numerics;
using Complex.Ecs;
using Complex.Ecs.Components;
using EngineKit.UI;
using ImGuiNET;

namespace Complex.Windows;

public class SceneHierarchyWindow : Window
{
    private readonly PropertyWindow _propertyWindow;

    private readonly Entity? _rootEntity;

    private readonly EntityId _rootEntityId;

    private readonly IScene _scene;

    private readonly IEntityWorld _world;

    private EntityId? _selectedEntityId;

    public SceneHierarchyWindow(IEntityWorld world,
                                IScene scene,
                                PropertyWindow propertyWindow)
    {
        _world = world;
        _scene = scene;
        _propertyWindow = propertyWindow;

        Caption = $"{MaterialDesignIcons.Tree} Hierarchy";
        OverwriteStyle = true;

        _rootEntityId = _scene.GetRoot();
        _rootEntity = _world.GetEntity(_rootEntityId);
    }

    public EntityId? SelectedEntityId
    {
        get => _selectedEntityId;
        set
        {
            if (!_selectedEntityId.Equals(value))
            {
                _selectedEntityId = value;
                _propertyWindow.SelectedEntityId = value;
            }
        }
    }

    protected override void DrawInternal()
    {
        if (_rootEntity == null) return;

        var isOpen = ImGui.TreeNodeEx(_rootEntity.Name, ImGuiTreeNodeFlags.SpanFullWidth);

        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            SelectedEntityId = _rootEntityId;
        }

        if (isOpen)
        {
            foreach (var child in _rootEntity.Children)
            {
                DrawChild(child);
            }

            ImGui.TreePop();
        }
    }

    protected override void SetStyleInternal()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
    }

    protected override void UnsetStyleInternal()
    {
        ImGui.PopStyleVar();
    }

    private void DrawChild(Entity child)
    {
        ImGui.PushID(child.ToString());

        var nameComponent = _world.GetComponent<NameComponent>(child.Id);
        var name = nameComponent?.Name ?? "UnnamedEntity";

        ImGui.PushID(name + child);
        var isOpen = ImGui.TreeNodeEx(name, ImGuiTreeNodeFlags.SpanFullWidth);

        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            SelectedEntityId = child.Id;
        }

        if (isOpen)
        {
            foreach (var grandChild in child.Children)
            {
                DrawChild(grandChild);
            }

            ImGui.TreePop();
        }

        ImGui.PopID();
    }
}
