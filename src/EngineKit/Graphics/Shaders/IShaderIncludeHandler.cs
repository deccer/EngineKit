namespace EngineKit.Graphics.Shaders;

public interface IShaderIncludeHandler
{
    string? HandleInclude(string? include);
}