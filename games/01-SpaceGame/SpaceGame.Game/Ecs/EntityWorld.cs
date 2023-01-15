using System;
using System.Collections.Generic;
using System.Linq;
using SpaceGame.Game.Ecs.Components;
using SpaceGame.Game.Ecs.Systems;
using SpaceGame.Game.Physics;

namespace SpaceGame.Game.Ecs;

public class EntityWorld : IEntityWorld
{
    private readonly IDictionary<Type, List<Component>> _componentsByType;
    private readonly IDictionary<int, Entity> _entities;
    private int _nextEntityId;
    private int _rootEntity;

    private readonly MovementSystem _movementSystem;
    private readonly TransformSystem _transformSystem;
    private readonly UpdateCameraSystem _updateCameraSystem;

    public EntityWorld(IPhysicsWorld physicsWorld, ICamera camera)
    {
        _componentsByType = new Dictionary<Type, List<Component>>();
        _entities = new Dictionary<int, Entity>();
        _nextEntityId = 0;

        _movementSystem = new MovementSystem(this);
        _transformSystem = new TransformSystem(this, physicsWorld);
        _updateCameraSystem = new UpdateCameraSystem(this, camera);

        _rootEntity = CreateEntity("Root");
    }

    public int CreateEntity(string name, int? parent = null)
    {
        var entity = new Entity(name)
        {
            Id = _nextEntityId++,
            Level = parent.HasValue ? GetEntity(parent.Value).Level + 1 : 0
        };

        _entities.Add(entity.Id, entity);

        if (parent != null)
        {
            AddComponent(entity.Id, new ParentComponent(parent.Value));
        }

        return entity.Id;
    }

    public Entity GetEntity(int entityId)
    {
        return _entities[entityId];
    }

    public void AddComponent<T>(int entityId, T component) where T : Component
    {
        var componentType = typeof(T);
        var entity = _entities[entityId];

        if (entity.Components.ContainsKey(componentType))
        {
            throw new Exception("Entity already has a component of type " + componentType.Name);
        }

        component.Entity = entity;
        entity.Components.Add(componentType, component);

        if (!_componentsByType.ContainsKey(componentType))
        {
            _componentsByType[componentType] = new List<Component>();
        }
        _componentsByType[componentType].Add(component);
    }

    public void RemoveComponent<T>(int entityId) where T : Component
    {
        var componentType = typeof(T);
        var entity = _entities[entityId];
        var component = GetComponent<T>(entity.Id);
        if (component == null)
        {
            throw new Exception("Entity does not have a component of type " + componentType.Name);
        }

        entity.Components.Remove(componentType);
        _componentsByType[componentType].Remove(component);
    }

    public List<Entity> GetEntitiesWithComponents<TComponent>()
        where TComponent : Component
    {
        var entities = new List<Entity>();
        if (!_componentsByType.TryGetValue(typeof(TComponent), out var components))
        {
            return entities;
        }

        foreach (var component in components)
        {
            if (!entities.Contains(component.Entity))
            {
                entities.Add(component.Entity);
            }
        }

        return entities;
    }

    public List<Entity> GetEntitiesWithComponents<TComponent1, TComponent2>()
        where TComponent1 : Component
        where TComponent2 : Component
    {
        var entities = new List<Entity>();

        if (!_componentsByType.TryGetValue(typeof(TComponent1), out var components1))
        {
            return entities;
        }

        if (!_componentsByType.TryGetValue(typeof(TComponent2), out var components2))
        {
            return entities;
        }

        foreach (var component1 in components1)
        {
            foreach (var component2 in components2)
            {
                if (component1.Entity == component2.Entity)
                {
                    entities.Add(component1.Entity);
                    break;
                }
            }
        }

        return entities;
    }

    public void Update(float deltaTime)
    {
        _movementSystem.Update(deltaTime);
        _transformSystem.Update(deltaTime);
        _updateCameraSystem.Update(deltaTime);
    }

    public T? GetComponent<T>(int entityId) where T : Component
    {
        var entity = _entities[entityId];
        if (entity.Components.ContainsKey(typeof(T)))
        {
            return (T)entity.Components[typeof(T)];
        }

        return null;
    }

    public IList<T> GetComponents<T>() where T : Component
    {
        var components = new List<T>();

        if (_componentsByType.ContainsKey(typeof(T)))
        {
            components = _componentsByType[typeof(T)].OfType<T>().ToList();
        }

        return components;
    }
}