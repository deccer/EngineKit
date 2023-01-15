namespace SpaceGame.Game.Ecs.Components;

public class ParentComponent : Component
{
    public ParentComponent(int parentId)
    {
        Parent = parentId;
    }

    public int Parent { get; set; }
}