namespace SpaceGame;

public struct GpuDirectionalShadowSettings
{
    public float Bias1; // 0.02
    public float Bias2; // 0.0015
    public float rMax; // 0.005;
    public float AccumFactor; // 1.0
    public int Samples; // 4
    public int RandomOffset; // 10000

    public int _padding1;
    public int _padding2;
}