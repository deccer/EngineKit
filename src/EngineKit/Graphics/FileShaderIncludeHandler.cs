using System;
using System.IO;

namespace EngineKit.Graphics;

public class FileShaderIncludeHandler : IShaderIncludeHandler
{
    private readonly string _baseDirectory;

    public FileShaderIncludeHandler()
    {
        _baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Shaders");
    }

    public string? HandleInclude(string? include)
    {
        if (string.IsNullOrEmpty(include))
        {
            return null;
        }

        var includeFilePath = Path.Combine(_baseDirectory, include);
        return File.Exists(includeFilePath)
            ? File.ReadAllText(includeFilePath)
            : null;
    }
}