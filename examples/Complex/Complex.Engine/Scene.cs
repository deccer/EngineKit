using System;
using System.Numerics;
using Complex.Engine.Ecs;
using Complex.Engine.Ecs.Components;
using Complex.Engine.Ecs.Systems;
using EngineKit.Graphics;

namespace Complex.Engine;

public class Scene : IScene
{
    private readonly IMaterialLibrary _materialLibrary;

    private readonly IModelLibrary _modelLibrary;

    private readonly EntityId _rootEntity;

    private readonly ISystemsUpdater _systemsUpdater;

    private readonly IEntityRegistry _registry;

    private bool _isDisposed;

    public Scene(IEntityRegistry registry,
                 IModelLibrary modelLibrary,
                 IMaterialLibrary materialLibrary,
                 ISystemsUpdater systemsUpdater)
    {
        _registry = registry;
        _modelLibrary = modelLibrary;
        _materialLibrary = materialLibrary;
        _systemsUpdater = systemsUpdater;
        _rootEntity = _registry.CreateEntity("Root");
        _registry.AddComponent(_rootEntity, new NameComponent("Root"));
    }

    public void Update(float deltaTime)
    {
        _systemsUpdater.Update(deltaTime);
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
        }
    }

    public EntityId GetRoot()
    {
        return _rootEntity;
    }

    public void AddEntityWithModelMeshRenderer(string name,
                                               EntityId? parent,
                                               ModelMesh modelMesh,
                                               Matrix4x4 startWorldMatrix)
    {
        var parentEntity = parent ?? _rootEntity;
        var modelEntityId = _registry.CreateEntity(name, parentEntity);
        var modelEntity = _registry.GetEntity(modelEntityId);

        modelEntity!.LocalMatrix = modelMesh.MeshPrimitive.Transform;

        _registry.AddComponent(modelEntityId, new NameComponent(name));
        _registry.AddComponent(modelEntityId, new ModelMeshComponent(modelMesh.MeshPrimitive));
        _registry.AddComponent(modelEntityId, new MaterialComponent(_materialLibrary.GetMaterialByName(modelMesh.MeshPrimitive.MaterialName)));
    }

    public void AddEntityWithModelRenderer(string name,
                                           EntityId? parent,
                                           Model model,
                                           Matrix4x4 startWorldMatrix)
    {
        var parentEntity = parent ?? _rootEntity;
        var modelEntityId = _registry.CreateEntity(name, parentEntity);
        var modelEntity = _registry.GetEntity(modelEntityId);
        modelEntity!.LocalMatrix = startWorldMatrix;

        _registry.AddComponent(modelEntityId, new NameComponent(name));

        foreach (var modelMesh in model.ModelMeshes)
        {
            var modelMeshName = string.IsNullOrEmpty(modelMesh.Name)
                ? $"Mesh-{Guid.NewGuid().ToString()}"
                : modelMesh.Name;
            var modelMeshEntityId = _registry.CreateEntity(modelMeshName, modelEntityId);
            var modelMeshEntity = _registry.GetEntity(modelMeshEntityId);
            modelMeshEntity!.LocalMatrix = modelMesh.MeshPrimitive.Transform;

            _registry.AddComponent(modelMeshEntityId, new NameComponent(modelMeshName));
            _registry.AddComponent(modelMeshEntityId, new ModelMeshComponent(modelMesh.MeshPrimitive));
            _registry.AddComponent(modelMeshEntityId, new MaterialComponent(_materialLibrary.GetMaterialByName(modelMesh.MeshPrimitive.MaterialName)));
        }
    }
}
