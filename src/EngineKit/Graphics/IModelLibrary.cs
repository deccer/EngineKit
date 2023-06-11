using System.Collections.Generic;

namespace EngineKit.Graphics;

public interface IModelLibrary
{
    void AddModel(Model model);

    void AddModelFromFile(string name, string filePath);

    Model? GetModelByName(string name);

    IReadOnlyCollection<string> GetModelNames();
}