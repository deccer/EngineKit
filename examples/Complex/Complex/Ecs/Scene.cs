using System.Numerics;
using Arch.Core;
using Arch.Relationships;
using BepuPhysics.Collidables;
using EngineKit.Graphics;

namespace Complex.Ecs;

public struct TransformComponent
{
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;

    public Matrix4x4 LocalWorldMatrix;
}

public struct NameComponent
{
    public NameComponent(string name)
    {
        Name = name;
    }

    public string Name;
}

public struct ModelComponent
{
    public ModelComponent(string modelName)
    {
        ModelName = modelName;
    }

    public string ModelName;
}

public struct ModelMeshComponent
{
    public string ModelMeshName;
    public MeshId? MeshId;
}

public struct MaterialComponent
{
    public string MaterialName;
    public MaterialId? MaterialId;
}

public struct ColliderComponent
{
    public Collidable Collidable;
}

public struct ParentOf
{
}

internal class Scene : IScene
{
    private readonly World _world;
    private bool _isDisposed;
    private readonly Entity _rootEntity;

    private readonly Arch.System.Group<float> _systems;

    public Scene()
    {
        _world = World.Create();
        _rootEntity = _world.Create(new NameComponent("Root"));

        _systems = new Arch.System.Group<float>(
            new PhysicsSystem(_world),
            new PreRenderSystem(_world));
    }
    
    public void Update(float deltaTime)
    {
        _systems.BeforeUpdate(deltaTime);
        _systems.Update(deltaTime);
        _systems.AfterUpdate(deltaTime);
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _systems.Dispose();
            _world.Dispose();
            _isDisposed = true;
        }
    }

    public Entity CreateEntity()
    {
        return _world.Create();
    }

    public Entity GetRoot()
    {
        return _rootEntity;
    }

    public void AddEntityWithModelRenderer(Entity? parent, Model model, Vector3 startPosition)
    {
        var parentEntity = parent ?? _rootEntity;
        var modelEntity = _world.Create(
            new NameComponent
            {
                Name = model.Name
            },
            new TransformComponent
            {
                Position = startPosition,
                Rotation = Quaternion.Identity,
                Scale = Vector3.One
            });
        parentEntity.AddRelationship<ParentOf>(modelEntity);
        foreach (var modelMesh in model.ModelMeshes)
        {
            var modelMeshEntity = _world.Create(
                new NameComponent
                {
                    Name = modelMesh.Name
                },
                new TransformComponent
                {
                    LocalWorldMatrix = modelMesh.MeshPrimitive.Transform
                },
                new ModelMeshComponent
                {
                    ModelMeshName = modelMesh.MeshPrimitive.MeshName
                });
            modelEntity.AddRelationship<ParentOf>(modelMeshEntity);
        }
    }
}