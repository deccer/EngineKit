namespace EngineKit.Graphics;

public interface ITextureLoader
{
    ITexture? LoadTextureFromFile(string filePath);
}