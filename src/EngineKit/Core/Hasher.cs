namespace EngineKit.Core;

public static class HashHelper
{
    public static uint XxHash(uint value)
    {
        const uint prime32One = 2246822519U;
        const uint prime32Three = 3266489917U;
        const uint prime32Four = 668265263U;
        const uint prime32Five = 374761393U;

        var h32 = value + prime32Five;
        h32 = prime32Four * ((h32 << 17) | (h32 >> (32 - 17)));
        h32 = prime32One * (h32 ^ (h32 >> 15));
        h32 = prime32Three * (h32 ^ (h32 >> 13));

        return h32 ^ (h32 >> 16);
    }
}
