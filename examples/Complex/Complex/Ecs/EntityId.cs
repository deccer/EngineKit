using System;

namespace Complex.Ecs;

public readonly struct EntityId : IEquatable<EntityId>
{
    private readonly int _entityId;
    
    public EntityId(int entityId)
    {
        _entityId = entityId;
    }

    public bool Equals(EntityId other)
    {
        return _entityId == other._entityId;
    }

    public override bool Equals(object? obj)
    {
        return obj is EntityId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _entityId;
    }
}