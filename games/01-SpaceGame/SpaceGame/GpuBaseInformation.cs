using System.Runtime.InteropServices;
using EngineKit.Mathematics;

namespace SpaceGame;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct GpuBaseInformation
{
    public Matrix ViewToClipMatrix;

    public Matrix ClipToViewMatrix;

    public Matrix WorldToViewMatrix;

    public Matrix ViewToWorldMatrix;

    public Vector4 CameraPosition;

    public Vector4 CameraDirection;
}