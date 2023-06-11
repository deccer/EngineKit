using System;

namespace EngineKit.Graphics.Shaders;

[AttributeUsage(AttributeTargets.Struct)]
public class UniformBufferAttribute : GlslAttribute
{
    public int Binding { get; set; }
}