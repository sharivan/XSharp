using System;
using System.Collections.Generic;

using XSharp.Factories;
using XSharp.Serialization;

namespace XSharp.Engine.Entities;

public interface IEntityReference : IIndexedNamedFactoryItemReference<Entity>
{
    IIndexedNamedFactory<Entity> IIndexedNamedFactoryItemReference<Entity>.Factory => BaseEngine.Engine.Entities;

    IIndexedFactory<Entity> IIndexedFactoryItemReference<Entity>.Factory => BaseEngine.Engine.Entities;

    INamedFactory<Entity> INamedFactoryItemReference<Entity>.Factory => BaseEngine.Engine.Entities;

    IFactory<Entity> IFactoryItemReference<Entity>.Factory => BaseEngine.Engine.Entities;

    IIndexedNamedFactory IIndexedNamedFactoryItemReference.Factory => BaseEngine.Engine.Entities;

    IIndexedFactory IIndexedFactoryItemReference.Factory => BaseEngine.Engine.Entities;

    INamedFactory INamedFactoryItemReference.Factory => BaseEngine.Engine.Entities;

    IFactory IFactoryItemReference.Factory => BaseEngine.Engine.Entities;

    new public Entity Target
    {
        get;
    }

    Entity IIndexedNamedFactoryItemReference<Entity>.Target => Target;

    Entity IIndexedFactoryItemReference<Entity>.Target => Target;

    Entity INamedFactoryItemReference<Entity>.Target => Target;

    Entity IFactoryItemReference<Entity>.Target => Target;

    IIndexedNamedFactoryItem IIndexedNamedFactoryItemReference.Target => Target;

    IIndexedFactoryItem IIndexedFactoryItemReference.Target => Target;

    INamedFactoryItem INamedFactoryItemReference.Target => Target;

    IFactoryItem IFactoryItemReference.Target => Target;

    Type IFactoryItemReference.ItemDefaultType => typeof(Entity);
}

public class EntityReference : IndexedNamedFactoryItemReference<Entity>, IEntityReference
{
    public override Entity Target
    {
        get
        {
            target ??= ((IIndexedNamedFactoryItemReference<Entity>) this).GetTargetFromIndex();
            return target;
        }
    }

    public override string TargetName
    {
        get => Target?.Name;

        set => throw new InvalidOperationException("Can't update target name of a entity reference.");
    }

    public override void Deserialize(ISerializer reader)
    {
        TargetIndex = reader.ReadInt();
    }

    public override void Serialize(ISerializer writer)
    {
        writer.WriteInt(TargetIndex);
    }

    public static implicit operator Entity(EntityReference reference)
    {
        return reference?.Target;
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
        return ReferenceEquals(obj, target)
            || obj != null
            && (
            obj is EntityReference reference && EqualityComparer<Entity>.Default.Equals(target, reference.Target)
            || obj is Entity entity && EqualityComparer<Entity>.Default.Equals(target, entity)
            );
    }

    protected override IndexedNamedFactory<Entity> GetFactory()
    {
        return BaseEngine.Engine.Entities;
    }

    public override int GetHashCode()
    {
        return TargetIndex;
    }

    public override void UpdateTargetNameFromIndex()
    {
    }

    public override void UpdateTargetIndexFromName()
    {
    }
}

public interface IEntityReference<EntityType> : IEntityReference where EntityType : Entity
{
    new public EntityType Target
    {
        get;
    }

    Entity IEntityReference.Target => Target;

    Entity IIndexedNamedFactoryItemReference<Entity>.Target => Target;

    Entity IIndexedFactoryItemReference<Entity>.Target => Target;

    Entity INamedFactoryItemReference<Entity>.Target => Target;

    Entity IFactoryItemReference<Entity>.Target => Target;

    IIndexedNamedFactoryItem IIndexedNamedFactoryItemReference.Target => Target;

    IIndexedFactoryItem IIndexedFactoryItemReference.Target => Target;

    INamedFactoryItem INamedFactoryItemReference.Target => Target;

    IFactoryItem IFactoryItemReference.Target => Target;

    Type IFactoryItemReference.ItemDefaultType => typeof(EntityType);
}

public class EntityReference<EntityType> : EntityReference, IEntityReference<EntityType> where EntityType : Entity
{
    new public EntityType Target => (EntityType) base.Target;

    public static implicit operator EntityType(EntityReference<EntityType> reference)
    {
        return reference?.Target;
    }

    public static implicit operator EntityReference<EntityType>(EntityType target)
    {
        return target?.Factory.GetReferenceTo(target);
    }

    public static bool operator ==(EntityReference<EntityType> reference1, EntityReference<EntityType> reference2)
    {
        return ReferenceEquals(reference1, reference2)
            || reference1 is not null && reference2 is not null && reference1.Equals(reference2);
    }

    public static bool operator ==(EntityReference<EntityType> reference, EntityType entity)
    {
        return reference is null ? entity is null : EqualityComparer<Entity>.Default.Equals(reference.Target, entity);
    }

    public static bool operator ==(EntityType entity, EntityReference<EntityType> reference)
    {
        return reference == entity;
    }

    public static bool operator !=(EntityReference<EntityType> reference1, EntityReference<EntityType> reference2)
    {
        return !(reference1 == reference2);
    }

    public static bool operator !=(EntityReference<EntityType> reference, EntityType entity)
    {
        return !(reference == entity);
    }

    public static bool operator !=(EntityType entity, EntityReference<EntityType> reference)
    {
        return !(entity == reference);
    }

    public override bool Equals(object? obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return TargetIndex;
    }
}

internal class EntityProxyReference<EntityType>(IEntityReference proxy) : EntityReference<EntityType> where EntityType : Entity
{
    private IEntityReference proxy = proxy;

    public override int TargetIndex
    {
        get => proxy != null ? proxy.TargetIndex : -1;

        set => throw new InvalidOperationException("Can't update the target index of a proxy reference.");
    }

    public override string TargetName => proxy?.TargetName;

    public override void Unset()
    {
        base.Unset();
        proxy = null;
    }
}