using System.Numerics;
using EngineKit.Mathematics;

namespace EngineKit;

public static class BoundingBoxHelper
{
    public static BoundingBox Transform(this BoundingBox boundingBox, Matrix4x4 worldMatrix)
    {
        var corners = boundingBox.GetCorners();
        for (var i = 0; i < corners.Length; i++)
        {
            corners[i] = Vector3.Transform(corners[i], worldMatrix);
        }

        var newMin = corners[0];
        var newMax = corners[0];

        for (var i = 1; i < corners.Length; i++)
        {
            newMin = Vector3.Min(newMin, corners[i]);
            newMax = Vector3.Max(newMax, corners[i]);
        }

        return new BoundingBox(newMin, newMax);
    }
}