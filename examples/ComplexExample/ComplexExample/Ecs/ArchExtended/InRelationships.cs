namespace ComplexExample.Ecs.ArchExtended;

internal readonly struct InRelationships
{
    /// <summary>
    ///     The buffer holding a relationship with the owning entity of this component.
    /// </summary>
    internal readonly IBuffer Relationships;

    internal InRelationships(IBuffer relationships)
    {
        Relationships = relationships;
    }
}