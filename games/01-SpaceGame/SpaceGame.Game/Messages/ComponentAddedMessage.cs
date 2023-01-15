using SpaceGame.Game.Ecs.Components;

namespace SpaceGame.Game.Messages;

public readonly struct ComponentAddedMessage<TComponent> where TComponent : Component
{
    public readonly TComponent Component;

    public ComponentAddedMessage(TComponent component)
    {
        Component = component;
    }
}