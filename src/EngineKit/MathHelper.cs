using System;

namespace EngineKit;

public static class MathHelper
{
    public static float ToRadians(float degrees)
    {
        return degrees * MathF.PI / 180f;
    }
}