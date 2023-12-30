using Complex.Ecs;
using Complex.Ecs.Components;
using EngineKit.UI;
using ImGuiNET;

namespace Complex.Windows;

public class SceneWindow : Window
{
    private readonly IEntityWorld _world;
    private readonly IScene _scene;
    private readonly PropertyWindow _propertyWindow;
    private EntityId? _selectedEntityId;

    private EntityId _rootEntityId;
    private Entity? _rootEntity;

    public SceneWindow(IEntityWorld world, IScene scene, PropertyWindow propertyWindow)
    {
        _world = world;
        _scene = scene;
        _propertyWindow = propertyWindow;

        Caption = $"{MaterialDesignIcons.Tree} Scene";

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
        if (_rootEntity == null)
        {
            return;
        }
        
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

    /*
    protected override void DrawInternal()
    {
        var rootEntities = _world.GetEntitiesWithComponents<RelationShipComponent>()
            .Where(e => _world.GetComponent<RelationShipComponent>(e.Id)?.Parent == null)
            .Select(e => e)
            .ToList();
        
        
        foreach (var rootEntity in rootEntities)
        {
            ImGui.PushID(rootEntity.GetHashCode());
            
            if (ImGui.Selectable(rootEntity.Name))
            {
                SelectedEntityId = rootEntity.Id;
            }
            else
            {
                SelectedEntityId = null;
            }
            
            ImGui.PopID();
        }
    }
    */
}