using System.Numerics;
using Complex.Ecs.Components;
using Complex.Ecs.Systems;
using EngineKit.Graphics;

namespace Complex.Ecs;

internal class Scene : IScene
{
    private readonly IEntityWorld _world;
    private readonly IModelLibrary _modelLibrary;
    private readonly IMaterialLibrary _materialLibrary;
    private readonly ISystemsUpdater _systemsUpdater;
    private readonly EntityId _rootEntity;
    private bool _isDisposed;

    public Scene(
        IEntityWorld world,
        IModelLibrary modelLibrary,
        IMaterialLibrary materialLibrary,
        ISystemsUpdater systemsUpdater)
    {
        _world = world;
        _modelLibrary = modelLibrary;
        _materialLibrary = materialLibrary;
        _systemsUpdater = systemsUpdater;
        _rootEntity = _world.CreateEntity("Root");
        _world.AddComponent(_rootEntity, new NameComponent("Root"));
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

    public void AddEntityWithModelMeshRenderer(
        string name,
        EntityId? parent,
        ModelMesh modelMesh,
        Matrix4x4 startWorldMatrix)
    {
        var parentEntity = parent ?? _rootEntity;
        var modelEntityId = _world.CreateEntity(name, parentEntity);
        var modelEntity = _world.GetEntity(modelEntityId);
        
        modelEntity!.LocalMatrix = modelMesh.MeshPrimitive.Transform;
        
        _world.AddComponent(modelEntityId, new NameComponent(name));
        _world.AddComponent(modelEntityId, new ModelMeshComponent(modelMesh.MeshPrimitive));
        _world.AddComponent(modelEntityId, new MaterialComponent(_materialLibrary.GetMaterialByName(modelMesh.MeshPrimitive.MaterialName)));
    }

    public void AddEntityWithModelRenderer(
        string name,
        EntityId? parent,
        Model model,
        Matrix4x4 startWorldMatrix)
    {
        var parentEntity = parent ?? _rootEntity;
        var modelEntityId = _world.CreateEntity(name, parentEntity);
        var modelEntity = _world.GetEntity(modelEntityId);
        modelEntity!.LocalMatrix = startWorldMatrix;
        
        _world.AddComponent(modelEntityId, new NameComponent(name));
        
        foreach (var modelMesh in model.ModelMeshes)
        {
            var modelMeshName = string.IsNullOrEmpty(modelMesh.Name)
                ? $"Mesh-{System.Guid.NewGuid().ToString()}"
                : modelMesh.Name;
            var modelMeshEntityId = _world.CreateEntity(modelMeshName, modelEntityId);
            var modelMeshEntity = _world.GetEntity(modelMeshEntityId);
            modelMeshEntity!.LocalMatrix = modelMesh.MeshPrimitive.Transform;
           
            _world.AddComponent(modelMeshEntityId, new NameComponent(modelMeshName));
            _world.AddComponent(modelMeshEntityId, new ModelMeshComponent(modelMesh.MeshPrimitive));
            _world.AddComponent(modelMeshEntityId, new MaterialComponent(_materialLibrary.GetMaterialByName(modelMesh.MeshPrimitive.MaterialName)));
        }
    }
}