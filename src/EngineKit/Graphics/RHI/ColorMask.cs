using System;

namespace EngineKit.Graphics.RHI;

[Flags]
public enum ColorMask : uint
{
    None = 0,
    Red = 1,
    Green = 2,
    Blue = 4,
    Alpha = 8,
    All = Red | Green | Blue | Alpha
}