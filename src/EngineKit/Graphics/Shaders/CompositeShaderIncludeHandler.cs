using System.Collections.Generic;
using System.Linq;

namespace EngineKit.Graphics.Shaders;

public class CompositeShaderIncludeHandler : IShaderIncludeHandler
{
    private readonly IShaderIncludeHandler[] _shaderIncludeHandlers;

    public CompositeShaderIncludeHandler(IEnumerable<IShaderIncludeHandler> shaderIncludeHandlers)
    {
        _shaderIncludeHandlers = shaderIncludeHandlers
            .Where(shaderIncludeHandler => shaderIncludeHandler.GetType() != typeof(CompositeShaderIncludeHandler))
            .ToArray();
    }

    public string? HandleInclude(string? include)
    {
        return _shaderIncludeHandlers.Aggregate(string.Empty, (current, handler) => current + handler.HandleInclude(include));
    }
}