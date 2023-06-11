using System.Runtime.InteropServices;
using EngineKit.Mathematics;

namespace EngineKit.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct GpuLight
{
    public Vector4 PositionAndType; // xyz = position, w = type
    
    public Vector4 Color; // xyz = color, w = radius
    
    public Vector4 Attenuation; // x = att.quadratic, y = att.linear, z = att.constant
    
    public Vector4 DirectionAndCutOff; // xyz = direction, w = cut off
}