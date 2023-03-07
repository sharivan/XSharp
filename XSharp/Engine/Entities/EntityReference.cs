using System;
using System.Collections.Generic;

namespace XSharp.Engine.Entities;

public interface IEntityReference
{
    public int TargetIndex
    {
        get;
        set;
    }

    public Entity Target
    {
        get;
        set;
    }
}

public abstract class EntityReference : IEntityReference
{
    public static EntityReference ReferenceTo(int index, Type fallBackEntityType)
    {
        if (index < 0)
            return null;

        EntityReference reference = GameEngine.Engine.entityReferences[index];
        if (reference is not null)
            return reference;

        Type referenceType = typeof(EntityIndexReference<>);

        var entity = GameEngine.Engine.entities[index];
        referenceType = entity == null ? referenceType.MakeGenericType(entity.GetType()) : referenceType.MakeGenericType(fallBackEntityType);
        reference = (EntityReference) Activator.CreateInstance(referenceType, true);
        reference.TargetIndex = index;

        return reference;
    }

    public static EntityReference ReferenceTo(Entity entity, Type fallBackEntityType)
    {
        return entity == null ? null : ReferenceTo(entity.Index, fallBackEntityType);
    }

    public abstract int TargetIndex
    {
        get;
        set;
    }

    public Entity Target
    {
        get => TargetIndex >= 0 ? GameEngine.Engine.entities[TargetIndex] : null;
        set => TargetIndex = value != null ? value.Index : -1;
    }

    public static implicit operator Entity(EntityReference reference)
    {
        return reference?.Target;
    }

    public static implicit operator EntityReference(Entity target)
    {
        return ReferenceTo(target, target != null ? target.GetType() : typeof(Entity));
    }

    public static bool operator ==(EntityReference reference1, EntityReference reference2)
    {
        return ReferenceEquals(reference1, reference2)
            || reference1 is not null && reference2 is not null && reference1.Equals(reference2);
    }

    public static bool operator ==(EntityReference reference, Entity entity)
    {
        return reference is null ? entity is null : EqualityComparer<Entity>.Default.Equals(reference.Target, entity);
    }

    public static bool operator ==(Entity entity, EntityReference reference)
    {
        return reference == entity;
    }

    public static bool operator !=(EntityReference reference1, EntityReference reference2)
    {
        return !(reference1 == reference2);
    }

    public static bool operator !=(EntityReference reference, Entity entity)
    {
        return !(reference == entity);
    }

    public static bool operator !=(Entity entity, EntityReference reference)
    {
        return !(entity == reference);
    }

    public override bool Equals(object? obj)
    {
        var target = Target;
        return obj == target
            || obj != null
            && (
            obj is EntityReference reference && EqualityComparer<Entity>.Default.Equals(target, reference.Target)
            || obj is Entity entity && EqualityComparer<Entity>.Default.Equals(target, entity)
            );
    }

    public override int GetHashCode()
    {
        return TargetIndex;
    }

    public override string ToString()
    {
        var target = Target;
        return target != null ? target.ToString() : "null";
    }
}

public interface IEntityReference<T> : IEntityReference where T : Entity
{
    new public T Target
    {
        get;
        set;
    }
}

public abstract class EntityReference<T> : EntityReference, IEntityReference<T> where T : Entity
{
    new public T Target
    {
        get => TargetIndex >= 0 ? (T) GameEngine.Engine.entities[TargetIndex] : null;
        set => TargetIndex = value != null ? value.Index : -1;
    }

    public static implicit operator T(EntityReference<T> reference)
    {
        return reference?.Target;
    }

    public static implicit operator EntityReference<T>(T target)
    {
        var reference = ReferenceTo(target, typeof(T));
        return reference is EntityIndexReference<T> referenceT ? referenceT : new EntityProxyReference<T>(reference);
    }
}

internal class EntityIndexReference<T> : EntityReference<T> where T : Entity
{
    internal int targetIndex = -1;

    public override int TargetIndex
    {
        get => targetIndex;

        set
        {
            if (targetIndex >= 0)
                GameEngine.Engine.entityReferences[targetIndex] = null;

            targetIndex = value;

            if (targetIndex >= 0)
                GameEngine.Engine.entityReferences[targetIndex] = this;
        }
    }

    internal EntityIndexReference()
    {
    }
}

internal class EntityProxyReference<T> : EntityReference<T> where T : Entity
{
    private IEntityReference proxy;

    public override int TargetIndex
    {
        get => proxy != null ? proxy.TargetIndex : -1;

        set
        {
            if (proxy != null)
                proxy.TargetIndex = value;
        }
    }

    internal EntityProxyReference(IEntityReference proxy)
    {
        this.proxy = proxy;
    }
}