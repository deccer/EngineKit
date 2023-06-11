using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace EngineKit.Graphics.Shaders;

internal sealed class ShaderParser : IShaderParser
{
    private static readonly Regex _includeRegex =
        new Regex("^[ ]*#[ ]*include[ ]+[\\\"<](?'include'.*)[\\\">].*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly IShaderIncludeHandler _includeHandler;

    public ShaderParser(IEnumerable<IShaderIncludeHandler> includeHandlers)
    {
        _includeHandler = new CompositeShaderIncludeHandler(includeHandlers);
    }

    public string ParseShader(string shaderSource)
    {
        var newShaderSourceLines = new StringBuilder();
        var shaderSourceLines = shaderSource.Split("\n");
        for (var i = 0; i < shaderSourceLines.Length; i++)
        {
            var shaderSourceLine = shaderSourceLines[i];
            var match = _includeRegex.Match(shaderSourceLine);
            if (match.Success)
            {
                var includeName = match.Groups["include"].Value;
                var replaceWithInclude = _includeHandler.HandleInclude(includeName);
                if (!string.IsNullOrEmpty(replaceWithInclude))
                {
                    newShaderSourceLines.AppendLine(replaceWithInclude);
                }
            }
            else
            {
                newShaderSourceLines.AppendLine(shaderSourceLine);
            }
        }

        return newShaderSourceLines.ToString();
    }
}