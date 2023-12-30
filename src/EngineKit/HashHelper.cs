namespace EngineKit;

public static class HashHelper
{
    public static uint XxHash(uint p)
    {
        const uint PRIME32_2 = 2246822519U, PRIME32_3 = 3266489917U;
        const uint PRIME32_4 = 668265263U, PRIME32_5 = 374761393U;

        uint h32 = p + PRIME32_5;
        h32 = PRIME32_4 * ((h32 << 17) | (h32 >> (32 - 17)));
        h32 = PRIME32_2 * (h32 ^ (h32 >> 15));
        h32 = PRIME32_3 * (h32 ^ (h32 >> 13));

        return h32 ^ (h32 >> 16);
    }
}