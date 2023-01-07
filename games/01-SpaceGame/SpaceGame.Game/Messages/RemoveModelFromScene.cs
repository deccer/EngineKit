namespace SpaceGame.Game.Messages;

public readonly struct RemoveModelFromScene
{
    public readonly Model Model;

    public RemoveModelFromScene(Model model)
    {
        Model = model;
    }
}