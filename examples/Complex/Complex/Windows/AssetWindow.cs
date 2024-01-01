using System.Numerics;
using EngineKit;
using EngineKit.Graphics;
using EngineKit.UI;
using ImGuiNET;

namespace Complex.Windows;

public class AssetWindow : Window
{
    private readonly IModelLibrary _modelLibrary;
    private readonly IMaterialLibrary _materialLibrary;
    private readonly IScene _scene;
    private readonly ICamera _camera;
    private readonly SceneHierarchyWindow _sceneHierarchyWindow;

    public AssetWindow(
        IModelLibrary modelLibrary,
        IMaterialLibrary materialLibrary,
        IScene scene,
        ICamera camera,
        SceneHierarchyWindow sceneHierarchyWindow)
    {
        _modelLibrary = modelLibrary;
        _materialLibrary = materialLibrary;
        _scene = scene;
        _camera = camera;
        _sceneHierarchyWindow = sceneHierarchyWindow;

        Caption = $"{MaterialDesignIcons.ViewDashboard} Assets";
    }

    protected override void DrawInternal()
    {
        var modelNames = _modelLibrary.GetModelNames();

        if (ImGui.BeginTable("Assets", 3, ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn("Model", ImGuiTableColumnFlags.NoSort);
            ImGui.TableSetupColumn("Mesh", ImGuiTableColumnFlags.NoSort);
            ImGui.TableSetupColumn("Instantiate", ImGuiTableColumnFlags.NoSort | ImGuiTableColumnFlags.WidthFixed, 32);
            ImGui.TableHeadersRow();

            foreach (var modelName in modelNames)
            {
                ImGui.TableNextRow();
                
                ImGui.PushID(modelName);

                var model = _modelLibrary.GetModelByName(modelName);
                
                ImGui.TableSetColumnIndex(0);
                var isExpanded = ImGui.TreeNodeEx(model!.Name);
                
                ImGui.TableSetColumnIndex(2);
                if (ImGui.Button($"{MaterialDesignIcons.Plus}"))
                {
                    _scene.AddEntityWithModelRenderer(model.Name, _sceneHierarchyWindow.SelectedEntityId, model, Matrix4x4.Identity);
                }
                
                if (isExpanded)
                {
                    foreach (var modelMesh in model.ModelMeshes)
                    {
                        ImGui.TableNextRow();
                        
                        ImGui.PushID(modelMesh.Name);
                        ImGui.TableSetColumnIndex(0);
                        ImGui.TextUnformatted(modelMesh.Name);
                        ImGui.TableSetColumnIndex(2);
                        if (ImGui.Button($"{MaterialDesignIcons.Plus}"))
                        {
                            _scene.AddEntityWithModelMeshRenderer(modelMesh.Name, _sceneHierarchyWindow.SelectedEntityId, modelMesh, Matrix4x4.Identity);
                        }
                        ImGui.PopID();
                    }

                    ImGui.TreePop();
                }

                ImGui.PopID();
            }
            
            ImGui.EndTable();
        }
    }
}