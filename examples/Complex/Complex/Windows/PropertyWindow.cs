using Complex.Ecs;
using Complex.Ecs.Components;
using EngineKit.Mathematics;
using EngineKit.UI;
using ImGuiNET;

namespace Complex.Windows;

public class PropertyWindow : Window
{
    private readonly IEntityWorld _world;

    private Entity? _selectedEntity;

    private EntityId? _selectedEntityId;

    public PropertyWindow(IEntityWorld world)
    {
        _world = world;
        Caption = $"{MaterialDesignIcons.Cards} Properties";

        _selectedEntityId = null;
        _selectedEntity = null;
    }

    public EntityId? SelectedEntityId
    {
        get => _selectedEntityId;
        set
        {
            if (!_selectedEntityId.Equals(value))
            {
                _selectedEntityId = value;
                if (_selectedEntityId.HasValue)
                {
                    _selectedEntity = _world.GetEntity(_selectedEntityId.Value);
                }
            }
        }
    }

    protected override void DrawInternal()
    {
        if (_selectedEntity == null || !_selectedEntityId.HasValue) return;

        ImGui.PushID(_selectedEntity.Name);

        if (ImGui.CollapsingHeader($"{MaterialDesignIcons.Compass} {_selectedEntity.Name}"))
        {
            var localPosition = _selectedEntity.Position;
            var localRotation = _selectedEntity.Rotation;
            var localScale = _selectedEntity.Scale;

            if (ImGui.DragFloat3("Position",
                    ref localPosition,
                    0.025f))
            {
                _selectedEntity.Position = localPosition;
            }

            if (ImGui.DragFloat3("Rotation",
                    ref localRotation,
                    0.025f))
            {
                _selectedEntity.Rotation = localRotation;
            }

            if (ImGui.DragFloat3("Scale",
                    ref localScale,
                    0.025f))
            {
                _selectedEntity.Scale = localScale;
            }
        }

        ImGui.PopID();

        var components = _world.GetAllComponents(_selectedEntityId.Value);
        foreach (var component in components)
        {
            var componentType = component.GetType();
            var componentName = componentType.Name;

            ImGui.PushID(componentName);

            if (component is NameComponent nameComponent)
            {
                if (ImGui.CollapsingHeader($"{MaterialDesignIcons.Label} Name"))
                {
                    ImGui.TextColored(Colors.ForestGreen, $"{MaterialDesignIcons.Label} Name");
                    ImGui.SameLine();
                    ImGui.TextUnformatted(nameComponent.Name);
                }
            }
            else
            {
                var isOpen = ImGui.TreeNodeEx(componentName);
                if (isOpen)
                {
                    ImGui.TreePop();
                }
            }

            ImGui.PopID();
        }
    }
}
