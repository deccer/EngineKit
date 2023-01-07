namespace SpaceGame;

public class RendererContext : IRendererContext
{
    public DrawMode DrawMode { get; set; }

    public bool UseWireframe { get; set; }
}