namespace EngineKit.Mathematics;

public static class Swapper
{
    public static void Swap<T>(ref T a, ref T b)
    {
        (a, b) = (b, a);
    }
}
