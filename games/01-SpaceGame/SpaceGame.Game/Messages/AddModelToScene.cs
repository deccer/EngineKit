namespace SpaceGame.Game.Messages;

public readonly struct AddModelToScene
{
    public readonly Model Model;

    public AddModelToScene(Model model)
    {
        Model = model;
    }
}