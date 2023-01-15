using System;
using System.Collections.Generic;
using SpaceGame.Game.Ecs.Components;

namespace SpaceGame.Game.Ecs;

public class Entity
{
    public Entity(string name)
    {
        Name = name;
        Components = new Dictionary<Type, Component>();
    }

    public int Id;

    public int Level;

    public string Name;

    public readonly Dictionary<Type, Component> Components;

    public T? GetComponent<T>() where T : Component
    {
        if (Components.TryGetValue(typeof(T), out var component))
        {
            return (T)component;
        }

        return null;
    }
}