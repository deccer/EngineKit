using System;

namespace EngineKit.Graphics.RHI;

[Flags]
public enum StorageAllocationFlags
{
    None = 0,
    Dynamic = 1,
    Client = 2,
    Mappable = 4
}