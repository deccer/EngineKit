using EngineKit.Graphics;
using ImGuiNET;
using Num = System.Numerics;

namespace ComplexExample.Extensions;

public static class ImGuiExtensions
{
    private static readonly Num.Vector2 _uv0 = new Num.Vector2(0, 1);
    private static readonly Num.Vector2 _uv1 = new Num.Vector2(1, 0);
    
    public static void ShowImage(IHasTextureId texture, Num.Vector2 textureSize)
    {
        ImGui.Image((nint)texture.Id, new Num.Vector2(textureSize.X, textureSize.Y), _uv0, _uv1);
    }
}
