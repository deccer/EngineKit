using System.Numerics;

namespace ComplexExample;

public struct CameraInformation
{
    public Matrix4x4 ProjectionMatrix;
    
    public Matrix4x4 ViewMatrix;
    
    public Vector4 Viewport;
    
    public Vector4 CameraPosition;
    
    public Vector4 CameraDirection;
}