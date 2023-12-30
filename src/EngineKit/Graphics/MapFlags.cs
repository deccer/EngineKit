using System;

namespace EngineKit.Graphics;

[Flags]
public enum MapFlags
{
    Read = 1 << 0,
    Write = 1 << 1,
    Persistent = 1 << 2,
    Coherent = 1 << 3,
}