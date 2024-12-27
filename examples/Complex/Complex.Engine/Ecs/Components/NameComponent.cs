namespace Complex.Engine.Ecs.Components;

public class NameComponent : Component
{
    public NameComponent(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
}
