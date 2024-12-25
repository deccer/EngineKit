using System;
using System.Collections.Generic;
using System.Numerics;
using Complex.Engine.Ecs.Components;

namespace Complex.Engine.Ecs;

public class Entity
{
    public readonly Dictionary<Type, Component> Components;

    private Matrix4x4 _globalMatrix;

    private Vector3 _localPosition;

    private Vector3 _localRotation;

    private Vector3 _localScale;

    private Entity? _parent;

    private bool _isDirty;

    public EntityId Id;

    public string Name;

    public Entity(string name,
                  Entity? parent)
    {
        Name = name;
        Components = new Dictionary<Type, Component>();
        Parent = parent;
        Children = new List<Entity>();

        _localPosition = Vector3.Zero;
        _localRotation = Vector3.Zero;
        _localScale = Vector3.One;

        LocalMatrix = Matrix4x4.Identity;
        _globalMatrix = Matrix4x4.Identity;
        _isDirty = true;
    }

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

    public Matrix4x4 LocalMatrix { get; set; }

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
        if(_isDirty)
        {
            UpdateGlobalMatrix();
            _isDirty = false;
        }
        return _globalMatrix;
    }

    private void UpdateGlobalMatrix()
    {
        if (_parent != null)
        {
            _globalMatrix = LocalMatrix * _parent.GetGlobalMatrix();
        }
        else
        {
            _globalMatrix = LocalMatrix;
        }
    }

    private void UpdateLocalMatrix()
    {
        LocalMatrix = Matrix4x4.CreateScale(_localScale) *
                      Matrix4x4.CreateFromYawPitchRoll(_localRotation.X,
                          _localRotation.Y,
                          _localRotation.Z) *
                      Matrix4x4.CreateTranslation(_localPosition);

        _isDirty = true;
    }
}
