using System;
using System.Runtime.InteropServices;
using SharpGLTF.Schema2;

namespace EngineKit.MeshLoaders;

public static class AccessorExtensions
{
    public static Span<T> AsSpan<T>(this Accessor? accessor) where T : unmanaged
    {
        if (accessor == null)
        {
            return default;
        }

        var slice = accessor.SourceBufferView.Content.Slice(accessor.ByteOffset, accessor.ByteLength);
        return MemoryMarshal.Cast<byte, T>(slice);
    }
}