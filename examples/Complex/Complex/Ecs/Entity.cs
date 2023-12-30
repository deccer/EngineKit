using System;
using System.Collections.Generic;
using System.Numerics;
using Complex.Ecs.Components;

namespace Complex.Ecs;

public class Entity
{
    private Entity? _parent;

    private Vector3 _localPosition;
    private Vector3 _localRotation;
    private Vector3 _localScale;

    private Matrix4x4 _localMatrix;
    private Matrix4x4 _globalMatrix;

    public Vector3 Position
    {
        get => _localPosition;
        set
        {
            if (_localPosition != value)
            {
                _localPosition = value;
                UpdateLocalMatrix();
            }
        }
    }

    public Vector3 Rotation
    {
        get => _localRotation;
        set
        {
            if (_localRotation != value)
            {
                _localRotation = value;
                UpdateLocalMatrix();
            }
        }
    }

    public Vector3 Scale
    {
        get => _localScale;
        set
        {
            if (_localScale != value)
            {
                _localScale = value;
                UpdateLocalMatrix();
            }
        }
    }

    public Matrix4x4 LocalMatrix
    {
        get => _localMatrix;
        set => _localMatrix = value;
    }

    public Entity(string name, Entity? parent)
    {
        Name = name;
        Components = new Dictionary<Type, Component>();
        Parent = parent;
        Children = new List<Entity>();

        _localPosition = Vector3.Zero;
        _localRotation = Vector3.Zero;
        _localScale = Vector3.One;
        
        _localMatrix = Matrix4x4.Identity;
        _globalMatrix = Matrix4x4.Identity;
    }

    public Entity? Parent
    {
        get => _parent;
        set
        {
            _parent?.Children.Remove(this);
            _parent = value;
            _parent?.Children.Add(this);
        }
    }

    public List<Entity> Children { get; }

    public EntityId Id;

    public string Name;

    public readonly Dictionary<Type, Component> Components;

    public T GetComponent<T>() where T : Component
    {
        return (T)Components[typeof(T)];
    }

    public void AddChild(Entity child)
    {
        Children.Add(child);
    }

    public void RemoveChild(Entity child)
    {
        Children.Remove(child);
    }

    public Matrix4x4 GetGlobalMatrix()
    {
        UpdateGlobalMatrix();
        return _globalMatrix;
    }

    private void UpdateGlobalMatrix()
    {
        if (_parent != null)
        {
            _globalMatrix = _localMatrix * _parent.GetGlobalMatrix();
        }
        else
        {
            _globalMatrix = _localMatrix;
        }
    }

    private void UpdateLocalMatrix()
    {
        _localMatrix = Matrix4x4.CreateScale(_localScale) *
                       Matrix4x4.CreateFromYawPitchRoll(_localRotation.X, _localRotation.Y, _localRotation.Z) *
                       Matrix4x4.CreateTranslation(_localPosition);
    }
}