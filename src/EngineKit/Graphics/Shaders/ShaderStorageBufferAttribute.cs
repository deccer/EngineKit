using System;

namespace EngineKit.Graphics.Shaders;

[AttributeUsage(AttributeTargets.Struct)]
public class ShaderStorageBufferAttribute : GlslAttribute
{
    public int Binding { get; set; }

    public bool ReadOnly { get; set; } = true;

    public string? ArrayName { get; set; }

    public string? Alias { get; set; }
}