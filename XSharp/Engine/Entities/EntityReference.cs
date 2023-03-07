using System;
using System.Collections.Generic;

namespace XSharp.Engine.Entities;

public class EntityReference
{
    internal int index = -1;

    public int Index
    {
        get => index;
        set
        {
            Entity?.references.Remove(this);
            index = value;
            Entity?.references.Add(this);
        }
    }

    public Entity Entity
    {
        get => Index >= 0 ? GameEngine.Engine.entities [Index] : null;
        set => Index = value != null ? value.Index : -1;
    }

    public EntityReference(int index)
    {
        Index = index;
    }

    public EntityReference(Entity entity)
    {
        Index = entity != null ? entity.Index : -1;
    }

    public static implicit operator Entity(EntityReference reference)
    {
        return reference?.Entity;
    }

    public static implicit operator EntityReference(Entity entity)
    {
        return entity != null ? new EntityReference(entity) : null;
    }

    public static bool operator ==(EntityReference reference1, EntityReference reference2)
    {
        return EqualityComparer<Entity>.Default.Equals(reference1, reference2);
    }

    public static bool operator ==(EntityReference reference, Entity entity)
    {
        return reference is null ? entity is null : EqualityComparer<Entity>.Default.Equals(reference.Entity, entity);
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
        return obj == Entity
            || obj != null
            && (
            obj is EntityReference reference && EqualityComparer<Entity>.Default.Equals(Entity, reference.Entity)
            || obj is Entity entity && EqualityComparer<Entity>.Default.Equals(Entity, entity)
            );
    }

    public override int GetHashCode()
    {
        return Index;
    }

    public override string ToString()
    {
        var entity = Entity;
        return entity != null ? entity.ToString() : "null";
    }
}

#pragma warning disable CS0660 // O tipo define os operadores == ou !=, mas não substitui o Object.Equals(object o)
#pragma warning disable CS0661 // O tipo define os operadores == ou !=, mas não substitui o Object.GetHashCode()
public class EntityReference<T> : EntityReference where T : Entity
#pragma warning restore CS0661 // O tipo define os operadores == ou !=, mas não substitui o Object.GetHashCode()
#pragma warning restore CS0660 // O tipo define os operadores == ou !=, mas não substitui o Object.Equals(object o)
{
    new public T Entity
    {
        get => (T) base.Entity;
        set => base.Entity = value;
    }

    public EntityReference(T entity) : base(entity)
    {
    }

    public static implicit operator T(EntityReference<T> reference)
    {
        return reference?.Entity;
    }

    public static implicit operator EntityReference<T>(T entity)
    {
        return entity != null ? new EntityReference<T>(entity) : null;
    }

    public static bool operator ==(EntityReference<T> reference1, EntityReference<T> reference2)
    {
        return (EntityReference) reference1 == (EntityReference) reference2;
    }

    public static bool operator ==(EntityReference<T> reference, T entity)
    {
        return reference == (Entity) entity;
    }

    public static bool operator ==(T entity, EntityReference<T> reference)
    {
        return reference == entity;
    }

    public static bool operator !=(EntityReference<T> reference1, EntityReference<T> reference2)
    {
        return !(reference1.Entity == reference2);
    }

    public static bool operator !=(EntityReference<T> reference, T entity)
    {
        return !(reference == entity);
    }

    public static bool operator !=(T entity, EntityReference<T> reference)
    {
        return !(entity == reference.Entity);
    }
}