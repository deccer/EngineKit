using System;

namespace ComplexExample;

public interface IScene : IDisposable
{
    bool Load();
    
    void DiscoverAssets();

    void Render();
    
    void RenderUi();
}