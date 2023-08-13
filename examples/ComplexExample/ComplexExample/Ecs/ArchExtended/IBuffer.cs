using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Arch.Core;

namespace ComplexExample.Ecs.ArchExtended;

/// <summary>
///     Interface implemented by <see cref="Relationship{T}"/>.
/// </summary>
internal interface IBuffer
{
    /// <summary>
    ///     Comparer used to sort <see cref="Entity"/> relationships.
    /// </summary>
    internal static readonly Comparer<Entity> Comparer = Comparer<Entity>.Create((a, b) => a.Id.CompareTo(b.Id));

    /// <summary>
    ///     The amount of relationships currently in the buffer.
    /// </summary>
    int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    /// <summary>
    ///     Removes the buffer as a component from the given world and entity.
    /// </summary>
    /// <param name="world"></param>
    /// <param name="source"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Destroy(World world, Entity source);

    /// <summary>
    ///     Removes the relationship targeting <see cref="target"/> from this buffer.
    /// </summary>
    /// <param name="target">The <see cref="Entity"/> in the relationship to remove.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void Remove(Entity target);
}