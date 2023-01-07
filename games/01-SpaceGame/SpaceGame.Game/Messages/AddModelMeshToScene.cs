namespace SpaceGame.Game.Messages;

public readonly struct AddModelMeshToScene
{
    public readonly ModelMesh ModelMesh;

    public AddModelMeshToScene(ModelMesh modelMesh)
    {
        ModelMesh = modelMesh;
    }
}