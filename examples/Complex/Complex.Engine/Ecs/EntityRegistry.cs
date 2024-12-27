using System;
using System.Collections.Generic;
using System.Linq;
using Complex.Engine.Ecs.Components;

namespace Complex.Engine.Ecs;

public class EntityRegistry : IEntityRegistry
{
    private readonly IDictionary<Type, List<Component>> _componentsByType;

    private readonly IDictionary<EntityId, Entity> _entities;

    private int _nextEntityId;

    public EntityRegistry()
    {
        _componentsByType = new Dictionary<Type, List<Component>>();
        _entities = new Dictionary<EntityId, Entity>();
        _nextEntityId = 0;
    }

    public event Action<Component>? ComponentAdded;

    public event Action<Component>? ComponentRemoved;

    public event Action<Component>? ComponentChanged;

    public EntityId CreateEntity(string name,
                                 EntityId? parent = null)
    {
        Entity? parentEntity = null;
        if (parent != null)
        {
            parentEntity = GetEntity(parent.Value);
        }

        var entity = new Entity(name, parentEntity)
        {
            Id = new EntityId(_nextEntityId++)
        };

        _entities.Add(entity.Id, entity);
        return entity.Id;
    }

    public Entity? GetEntity(EntityId entityId)
    {
        return _entities.TryGetValue(entityId, out var entity) ? entity : null;
    }

    public void AddComponent<T>(EntityId entityId,
                                T component) where T : Component
    {
        var componentType = typeof(T);
        var entity = _entities[entityId];

        if (entity.Components.ContainsKey(componentType))
        {
            throw new Exception("Entity already has a component of type " + componentType.Name);
        }

        component.Entity = entity;
        entity.Components.Add(componentType, component);

        if (!_componentsByType.TryGetValue(componentType, out var components))
        {
            components = new List<Component>();
            _componentsByType[componentType] = components;
        }

        components.Add(component);

        var componentAdded = ComponentAdded;
        componentAdded?.Invoke(component);
    }

    public void RemoveComponent<T>(EntityId entityId) where T : Component
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

        var componentRemoved = ComponentRemoved;
        componentRemoved?.Invoke(component);
    }

    public List<Entity> GetEntitiesWithComponents<TComponent>()
            where TComponent : Component
    {
        if (!_componentsByType.TryGetValue(typeof(TComponent), out var components))
        {
            return Enumerable.Empty<Entity>().ToList();
        }

        var entities = new List<Entity>(256);

        foreach (var component in components)
        {
            if(component.Entity != null && !entities.Contains(component.Entity))
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
        if (!_componentsByType.TryGetValue(typeof(TComponent1), out var components1))
        {
            return Enumerable.Empty<Entity>().ToList();
        }

        if (!_componentsByType.TryGetValue(typeof(TComponent2), out var components2))
        {
            return Enumerable.Empty<Entity>().ToList();
        }

        var entities = new List<Entity>(256);

        foreach (var component1 in components1)
        {
            if (components2.Any(component2 => component1.Entity == component2.Entity))
            {
                if (component1.Entity != null)
                {
                    entities.Add(component1.Entity);
                }
            }
        }

        return entities;
    }

    public T? GetComponent<T>(EntityId entityId) where T : Component
    {
        var entity = _entities[entityId];
        if (entity.Components.ContainsKey(typeof(T)))
        {
            return (T)entity.Components[typeof(T)];
        }

        return null;
    }

    public List<T> GetComponents<T>() where T : Component
    {
        var components = new List<T>();

        if (_componentsByType.ContainsKey(typeof(T)))
        {
            components = _componentsByType[typeof(T)].OfType<T>().ToList();
        }

        return components;
    }

    public Dictionary<Type, Component>.ValueCollection GetAllComponents(EntityId entityId)
    {
        var entity = _entities[entityId];
        return entity.Components.Values;
    }
}
