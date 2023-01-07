using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace SpaceGame;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct GpuBaseInformation
{
    public Matrix4 ViewToClipMatrix;

    public Matrix4 ClipToViewMatrix;

    public Matrix4 WorldToViewMatrix;

    public Matrix4 ViewToWorldMatrix;

    public Vector4 CameraPosition;

    public Vector4 CameraDirection;
}