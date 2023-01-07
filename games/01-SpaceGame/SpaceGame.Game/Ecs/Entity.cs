using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace SpaceGame.Game.Ecs;

public class Entity
{
    private Entity? _parent;

    public Entity(string name, Entity? parent)
    {
        Name = name;
        Components = new Dictionary<Type, Component>();
        Parent = parent;
        Children = new List<Entity>();
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

    public IList<Entity> Children { get; }

    public int Id;

    public string Name;

    public readonly Dictionary<Type, Component> Components;

    public T GetComponent<T>() where T : Component
    {
        return (T)Components[typeof(T)];
    }

    public void UpdateTransforms(Vector3 position)
    {
        var transform = GetComponent<TransformComponent>();
        transform.LocalPosition = position;

        Matrix4 matrix = default;
        Matrix3.CreateFromQuaternion(in transform.LocalRotation, out var rotationMatrix);

        matrix.Row0 = new Vector4(rotationMatrix.Column0 * transform.LocalScale.X, 0f);
        matrix.Row1 = new Vector4(rotationMatrix.Column1 * transform.LocalScale.Y, 0f);
        matrix.Row2 = new Vector4(rotationMatrix.Column2 * transform.LocalScale.Z, 0f);
        matrix.Row3 = new Vector4(transform.LocalPosition, 1f);
        if (_parent != null)
        {
            _parent.UpdateTransforms();
            var parentTransform = Parent.GetComponent<TransformComponent>();
            var parentMatrix = parentTransform.GlobalWorldMatrix;
            Matrix4.Mult(matrix, parentMatrix, out matrix);
        }

        transform.GlobalWorldMatrix = matrix;
    }

    public void UpdateTransforms()
    {
        var transform = GetComponent<TransformComponent>();

        Matrix4 matrix = default;
        Matrix3.CreateFromQuaternion(in transform.LocalRotation, out var rotationMatrix);

        matrix.Row0 = new Vector4(rotationMatrix.Column0 * transform.LocalScale.X, 0f);
        matrix.Row1 = new Vector4(rotationMatrix.Column1 * transform.LocalScale.Y, 0f);
        matrix.Row2 = new Vector4(rotationMatrix.Column2 * transform.LocalScale.Z, 0f);
        matrix.Row3 = new Vector4(transform.LocalPosition, 1f);
        if (_parent != null)
        {
            _parent.UpdateTransforms();
            var parentTransform = Parent.GetComponent<TransformComponent>();
            var parentMatrix = parentTransform.GlobalWorldMatrix;
            Matrix4.Mult(matrix, parentMatrix, out matrix);
        }

        transform.GlobalWorldMatrix = matrix;
    }
}