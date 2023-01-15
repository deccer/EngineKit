using System.Collections.Generic;

namespace SpaceGame.Game.Ecs.Components;

public class ChildrenComponent : Component
{
    private readonly IList<int> _children;

    public ChildrenComponent()
    {
        _children = new List<int>();
    }

    public ICollection<int> Children => _children;

    public void AddChild(int child)
    {
        _children.Add(child);
    }

    public void RemoveChild(int child)
    {
        _children.Remove(child);
    }
}