using System.Numerics;

namespace Complex;

public struct CameraInformation
{
    public Matrix4x4 ProjectionMatrix;
    
    public Matrix4x4 ViewMatrix;
}