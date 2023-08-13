using EngineKit.Mathematics;

namespace ComplexExample;

public struct CameraInformation
{
    public Matrix ProjectionMatrix;
    
    public Matrix ViewMatrix;
    
    public Vector4 Viewport;
    
    public Vector4 CameraPosition;
    
    public Vector4 CameraDirection;
}