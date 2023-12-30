namespace Complex.Ecs.Components;

public class TagComponent<TTag> : Component where TTag : struct
{
    public TagComponent(TTag tag)
    {
        Tag = tag;
    }

    public TTag Tag { get; }
}