using System;

namespace EngineKit.Graphics.RHI;

[Flags]
public enum BufferStorageFlags
{
    None = 0,
    DynamicStorage = 1 << 0,
    ClientStorage = 1 << 1,
    MemoryMapped = 1 << 2
}