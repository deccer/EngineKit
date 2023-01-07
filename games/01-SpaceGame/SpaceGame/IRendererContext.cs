namespace SpaceGame;

public interface IRendererContext
{
    DrawMode DrawMode { get; set; }

    bool UseWireframe { get; set; }
}