using System.IO;
using System.Linq;
using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using ComplexExample.Ecs;
using ComplexExample.Ecs.ArchExtended;
using EngineKit;
using EngineKit.Graphics;
using EngineKit.Mathematics;
using EngineKit.UI;
using ImGuiNET;
using Serilog;

namespace ComplexExample;

public class Scene : IScene
{
    private static readonly Vector4 HeaderSelectedColor = new Vector4(0.8f, 0.6f, 0.1f, 1.0f);
    private readonly ILogger _logger;
    private readonly IGraphicsContext _graphicsContext;
    private readonly IApplicationContext _applicationContext;
    private readonly IModelLibrary _modelLibrary;
    private readonly IMaterialLibrary _materialLibrary;
    private readonly IRenderer _renderer;
    private readonly ICamera _camera;
    private readonly IMeshPool? _meshPool;

    private readonly World _ecsWorld;
    private readonly QueryDescription _updateTransformQuery;
    private readonly QueryDescription _collectAllRenderablesQuery;
    private readonly QueryDescription _sceneHierarchyQuery;
    private readonly Entity _rootEntity;

    private SwapchainDescriptor _swapchainDescriptor;

    public string? SelectedAsset { get; set; }
    
    public Entity? SelectedEntity { get; set; }

    public Scene(
        ILogger logger,
        IGraphicsContext graphicsContext,
        IApplicationContext applicationContext,
        IModelLibrary modelLibrary,
        IMaterialLibrary materialLibrary,
        IRenderer renderer,
        ICamera camera)
    {
        _logger = logger;
        _graphicsContext = graphicsContext;
        _applicationContext = applicationContext;
        _modelLibrary = modelLibrary;
        _materialLibrary = materialLibrary;
        _renderer = renderer;
        _camera = camera;
        _ecsWorld = World.Create();
        
        _updateTransformQuery = new QueryDescription().WithAll<TransformComponent, Relationship<ParentOf>>();
        _collectAllRenderablesQuery = new QueryDescription().WithAll<TransformComponent, MeshComponent, MaterialComponent>();
        _sceneHierarchyQuery = new QueryDescription().WithAll<Relationship<ParentOf>>();
        _rootEntity = _ecsWorld.Create();
        _rootEntity.Add(new TransformComponent());
        _rootEntity.Add(new NameComponent { Name = "Root" });
    }

    public void Dispose()
    {
        _ecsWorld.Dispose();
    }

    public bool Load()
    {
        if (!_renderer.Load())
        {
            return false;
        }

        _swapchainDescriptor = new SwapchainDescriptorBuilder()
            .ClearColor(Colors.Black)
            .ClearDepth()
            .WithViewport(_applicationContext.FramebufferSize.X, _applicationContext.FramebufferSize.Y)
            .Build("Swapchain");

        return true;
    }

    public void DiscoverAssets()
    {
        var discoveredFiles = Directory
            .EnumerateFileSystemEntries("Data", "*.gl*", SearchOption.AllDirectories)
            .ToList();
        foreach (var discoveredFile in discoveredFiles)
        {
            _modelLibrary.AddModelFromFile(Path.GetFileNameWithoutExtension(discoveredFile), discoveredFile);
        }
    }

    public void Render()
    {
        // update transform hierarchies
        _ecsWorld.Query<Entity>(in _updateTransformQuery, (ref Entity entity) =>
        {
            ref var parentOfRelation = ref entity.GetRelationships<ParentOf>();
            ref var parentTransform = ref _ecsWorld.Get<TransformComponent>(entity);
            foreach (var child in parentOfRelation)
            {
                ref var childTransform = ref _ecsWorld.Get<TransformComponent>(child.Key);
                childTransform.GlobalMatrix = parentTransform.LocalMatrix * childTransform.LocalMatrix;
            }
        });

        // queue renderables
        _renderer.ClearRenderQueue();
        _ecsWorld.Query(in _collectAllRenderablesQuery, (ref TransformComponent transform, ref MeshComponent meshComponent, ref MaterialComponent materialComponent) =>
        {
            _renderer.AddToRenderQueue(meshComponent.Mesh, materialComponent.Material, transform.GlobalMatrix);
        });
        
        // render
        _renderer.RenderWorld(_camera);
    }

    public void RenderUi()
    {
        _graphicsContext.BeginRenderPass(_swapchainDescriptor);
        ImGui.ShowDemoWindow();
        RenderUiAssets();
        RenderUiScene();
        RenderUiSceneOutline();

        RenderUiSceneStatistics();
        RenderUiRenderSettings();
        _graphicsContext.EndRenderPass();
    }

    private void RenderUiAssets()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        if (ImGui.Begin($" {MaterialDesignIcons.WhiteBalanceSunny} Assets"))
        {
            var tableFlags = ImGuiTableFlags.BordersV | 
                             ImGuiTableFlags.BordersOuterH | 
                             ImGuiTableFlags.Resizable | 
                             ImGuiTableFlags.RowBg | 
                             ImGuiTableFlags.NoBordersInBody;
            if (ImGui.BeginTable("AssetTable", 2, tableFlags))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn($"{MaterialDesignIcons.EyeOutline}", ImGuiTableColumnFlags.WidthFixed, 72);
                ImGui.TableHeadersRow();
                
                var modelNames = _modelLibrary.GetModelNames();
                foreach (var modelName in modelNames)
                {
                    var model = _modelLibrary.GetModelByName(modelName);
                    RenderUiAssetModel(model);
                }
                
                ImGui.EndTable();
            }

            ImGui.End();
        }
        ImGui.PopStyleVar();
    }

    private void RenderUiAssetModel(Model model)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();

        var isFolder = model.ModelMeshes.Count() > 1;
        if (isFolder)
        {
            var label = $" {MaterialDesignIcons.CubeOutline} {model.Name}";
            var isOpen = ImGui.TreeNodeEx(label, ImGuiTreeNodeFlags.SpanFullWidth);

            ImGui.TableNextColumn();
            if (ImGui.SmallButton($"Spawn##{label.GetHashCode()}"))
            {
                AddModelInstance(model);
            }
            
            if (isOpen)
            {
                foreach (var modelMesh in model.ModelMeshes)
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    
                    label = $" {MaterialDesignIcons.CubeOutline} {modelMesh.MeshPrimitive.MeshName}";
                    var flags = ImGuiTreeNodeFlags.Leaf |
                                ImGuiTreeNodeFlags.NoTreePushOnOpen |
                                ImGuiTreeNodeFlags.SpanFullWidth;
                    ImGui.TreeNodeEx(label, flags);
                    ImGui.TableNextColumn();
                    if (ImGui.SmallButton($"Spawn##{label.GetHashCode()}"))
                    {
                        AddModelMeshInstance(modelMesh);
                    }
                }
                
                ImGui.TreePop();
            }            
        }
        else
        {
            var flags = ImGuiTreeNodeFlags.Leaf |
                        ImGuiTreeNodeFlags.NoTreePushOnOpen |
                        ImGuiTreeNodeFlags.SpanFullWidth;

            var label = $" {MaterialDesignIcons.CubeOutline} {model.Name}";
            ImGui.TreeNodeEx(label, flags);
            ImGui.TableNextColumn();
            if (ImGui.SmallButton($"Spawn##{label.GetHashCode()}"))
            {
                AddModelInstance(model);
            }  
        }
    }

    private void RenderUiScene()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        if (ImGui.Begin($" {MaterialDesignIcons.AllInclusive} Scene", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
        {
            _renderer.ShowScene();
        }
        ImGui.PopStyleVar();
    }

    private void RenderUiSceneOutline()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        if (ImGui.Begin($" {MaterialDesignIcons.Application} Outline"))
        {

            var tableFlags = ImGuiTableFlags.BordersV | 
                             ImGuiTableFlags.BordersOuterH | 
                             ImGuiTableFlags.Resizable | 
                             ImGuiTableFlags.RowBg | 
                             ImGuiTableFlags.NoBordersInBody | 
                             ImGuiTableFlags.NoPadInnerX | 
                             ImGuiTableFlags.NoPadOuterX;
            if (ImGui.BeginTable("HierarchyTable", 2, tableFlags))
            {
                ImGui.TableSetupColumn(string.Empty, ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn($"{MaterialDesignIcons.EyeOutline}", ImGuiTableColumnFlags.WidthFixed, 72);
                ImGui.TableHeadersRow();

                _ecsWorld.Query<Entity>(_sceneHierarchyQuery, (ref Entity entity) =>
                {
                    ref var nameComponent = ref entity.Get<NameComponent>();
                    var name = string.IsNullOrEmpty(nameComponent.Name)
                        ? $"Unnamed {nameComponent.GetHashCode()}"
                        : nameComponent.Name;
                    
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    var label = $" {MaterialDesignIcons.Cube} {name}###{entity.GetHashCode()}";
                    var isOpen = ImGui.TreeNodeEx(label, ImGuiTreeNodeFlags.SpanFullWidth);
                    if (isOpen)
                    {
                        ref var parentOfRelation = ref entity.GetRelationships<ParentOf>();
                        foreach (var child in parentOfRelation)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();                            
                            ref var childNameComponent = ref _ecsWorld.Get<NameComponent>(child.Key);
                            var childName = string.IsNullOrEmpty(childNameComponent.Name)
                                ? $"Unnamed {childNameComponent.GetHashCode()}"
                                : childNameComponent.Name;
                                
                            var flags = ImGuiTreeNodeFlags.Leaf |
                                        ImGuiTreeNodeFlags.NoTreePushOnOpen |
                                        ImGuiTreeNodeFlags.SpanFullWidth;

                            label = $" {MaterialDesignIcons.CubeOutline} {childName}###{child.GetHashCode()}";
                            ImGui.TreeNodeEx(label, flags);
                            if (ImGui.IsItemHovered())
                            {
                                SelectedEntity = entity;
                            }

                            ImGui.TableNextColumn();
                            ImGui.TextUnformatted($"{MaterialDesignIcons.Eye}");
                        }
                            
                        ImGui.TreePop();

                    }
                    else
                    {
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted($"{MaterialDesignIcons.Adobe}");
                    }

                    if (SelectedEntity.HasValue)
                    {
                        RenderUiProperties(SelectedEntity.Value);
                    }
                });
                
                ImGui.EndTable();
            }

            ImGui.End();
        }
        ImGui.PopStyleVar(1);
    }

    private void RenderUiSceneStatistics()
    {
        if (ImGui.Begin($" {MaterialDesignIcons.Information} Statistics"))
        {
            ImGui.End();
        }
    }

    private void RenderUiRenderSettings()
    {
        if (ImGui.Begin($" {MaterialDesignIcons.Gpu} Render Settings"))
        {
            ImGui.End();
        }
    }

    private void RenderUiProperties(Entity entity)
    {
        if (ImGui.Begin($" {MaterialDesignIcons.ViewList} Properties"))
        {
            var components = entity.GetAllComponents();
            //foreach (var component in components)
            for (var i = 0; i < components.Length; i++)
            {
                ref var component = ref components[i];
                
                if (component is NameComponent nameComponent)
                {
                    if (ImGui.CollapsingHeader($" {MaterialDesignIcons.Label} Name"))
                    {
                        ImGui.BeginTable($"NameTable###{component.GetHashCode()}", 2);
                        ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
                        ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch);
                        ImGui.TableHeadersRow();
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted("Name");
                        ImGui.TextUnformatted(nameComponent.Name);
                        ImGui.EndTable();
                    }
                }

                if (component is TransformComponent transformComponent)
                {
                    if (ImGui.CollapsingHeader($" {MaterialDesignIcons.VectorLine} Transform"))
                    {
                        ImGui.BeginTable($"NameTable###{component.GetHashCode()}", 2);
                        ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
                        ImGui.TableSetupColumn(string.Empty, ImGuiTableColumnFlags.WidthStretch);
                        ImGui.TableHeadersRow();
                        
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted("Position");
                        ImGui.TableNextColumn();
                        var position = transformComponent.Position;
                        ImGui.InputFloat3($"###Position{transformComponent.GetHashCode()}", ref position);

                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted("Rotation");
                        ImGui.TableNextColumn();
                        var rotation = new Vector3();
                        ImGui.InputFloat3($"###Rotation{transformComponent.GetHashCode()}", ref rotation);
                        
                        ImGui.TextUnformatted("Scale");
                        var scale = transformComponent.Scale;
                        ImGui.InputFloat3($"###Scale{transformComponent.GetHashCode()}", ref scale);

                        ImGui.EndTable();
                    }
                }

                if (component is MeshComponent meshComponent)
                {
                    if (ImGui.CollapsingHeader($" {MaterialDesignIcons.VectorPolygon} Mesh"))
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted("VertexOffset");
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(meshComponent.Mesh.VertexOffset.ToString());

                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted("VertexCount");
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(meshComponent.Mesh.VertexCount.ToString());

                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted("IndexOffset");
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(meshComponent.Mesh.IndexOffset.ToString());

                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted("IndexCount");
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(meshComponent.Mesh.IndexCount.ToString());
                    }
                }

                if (component is MaterialComponent materialComponent)
                {
                }
            }

            ImGui.End();
        }
    }

    private void AddModelInstance(Model model)
    {
        var entity = _ecsWorld.Create();
        entity.Add(new TransformComponent(Matrix4x4.Identity));
        entity.Add(new NameComponent(model.Name));
        _rootEntity.AddRelationship(entity, new ParentOf());

        foreach (var modelMesh in model.ModelMeshes)
        {
            var material = _materialLibrary.GetMaterialByName(modelMesh.MeshPrimitive.MaterialName);
            var pooledMesh = _renderer.AddMeshPrimitive(modelMesh.MeshPrimitive);
            var pooledMaterial = _renderer.AddMaterial(material);            
            
            var child = _ecsWorld.Create();
            child.Add(new TransformComponent(modelMesh.MeshPrimitive.Transform));
            child.Add(new MeshComponent(pooledMesh));
            child.Add(new MaterialComponent(pooledMaterial));
            child.Add(new NameComponent(modelMesh.Name));
            
            entity.AddRelationship(child,new ParentOf());
        }
    }

    private void AddModelMeshInstance(ModelMesh modelMesh)
    {
        var material = _materialLibrary.GetMaterialByName(modelMesh.MeshPrimitive.MaterialName);
        var pooledMesh = _renderer.AddMeshPrimitive(modelMesh.MeshPrimitive);
        var pooledMaterial = _renderer.AddMaterial(material);            
            
        var entity = _ecsWorld.Create();
        entity.Add(new TransformComponent(modelMesh.MeshPrimitive.Transform));
        entity.Add(new MeshComponent(pooledMesh));
        entity.Add(new MaterialComponent(pooledMaterial));
        entity.Add(new NameComponent(modelMesh.Name));
        _rootEntity.AddRelationship(entity, new ParentOf());
    }
}