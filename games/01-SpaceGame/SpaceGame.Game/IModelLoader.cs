namespace SpaceGame.Game;

public interface IModelLoader
{
    Model LoadModel(string name, string filePath);
}