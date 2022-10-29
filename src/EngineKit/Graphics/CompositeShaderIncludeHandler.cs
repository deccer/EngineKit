using System.Linq;

namespace EngineKit.Graphics;

public class CompositeShaderIncludeHandler : IShaderIncludeHandler
{
    private readonly IShaderIncludeHandler[] _shaderIncludeHandlers;

    public CompositeShaderIncludeHandler(params IShaderIncludeHandler[] shaderIncludeHandlers)
    {
        _shaderIncludeHandlers = shaderIncludeHandlers;
    }

    public string? HandleInclude(string? include)
    {
        return _shaderIncludeHandlers.Aggregate(string.Empty, (current, handler) => current + handler.HandleInclude(include));
    }
}