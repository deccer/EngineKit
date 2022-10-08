using System;
using ImGuiNET;

namespace EngineKit.Graphics;

public interface IUIRendererLoader
{
    bool Load(int width, int height, Action<ImGuiIOPtr>? configureIo = null);
}