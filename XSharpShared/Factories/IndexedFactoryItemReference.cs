using System.Collections.Generic;

using XSharp.Serialization;

namespace XSharp.Factories;

public abstract class IndexedFactoryItemReference<ItemType> : IIndexedFactoryItemReference<ItemType> where ItemType : IIndexedFactoryItem
{
    internal int targetIndex = -1;
    internal ItemType target = default;

    //public IndexedFactoryContainer<ItemType> Factory => GetFactory();

    public IIndexedFactory<ItemType> Factory => GetFactory();

    public virtual ItemType Target
    {
        get
        {
            target ??= ((IIndexedFactoryItemReference<ItemType>) this).GetTargetFromIndex();
            return target;
        }
    }

    public virtual int TargetIndex => targetIndex;

    protected IndexedFactoryItemReference()
    {
    }

    protected abstract IIndexedFactory<ItemType> GetFactory();

    public virtual void Deserialize(ISerializer reader)
    {
        targetIndex = reader.ReadInt();
    }

    public virtual void Serialize(ISerializer writer)
    {
        writer.WriteInt(targetIndex);
    }

    public static implicit operator ItemType(IndexedFactoryItemReference<ItemType> reference)
    {
        return reference != null ? reference.Target : default;
    }

    public static bool operator ==(IndexedFactoryItemReference<ItemType> reference1, IndexedFactoryItemReference<ItemType> reference2)
    {
        return ReferenceEquals(reference1, reference2)
            || reference1 is not null && reference2 is not null && reference1.Equals(reference2);
    }

    public static bool operator ==(IndexedFactoryItemReference<ItemType> reference, ItemType item)
    {
        return reference is null ? item is null : EqualityComparer<ItemType>.Default.Equals(reference.Target, item);
    }

    public static bool operator ==(ItemType item, IndexedFactoryItemReference<ItemType> reference)
    {
        return reference == item;
    }

    public static bool operator !=(IndexedFactoryItemReference<ItemType> reference1, IndexedFactoryItemReference<ItemType> reference2)
    {
        return !(reference1 == reference2);
    }

    public static bool operator !=(IndexedFactoryItemReference<ItemType> reference, ItemType item)
    {
        return !(reference == item);
    }

    public static bool operator !=(ItemType item, IndexedFactoryItemReference<ItemType> reference)
    {
        return !(item == reference);
    }

    public override bool Equals(object? obj)
    {
        var target = Target;
        return ReferenceEquals(obj, target)
            || obj != null
            && (
            obj is IndexedFactoryItemReference<ItemType> reference && EqualityComparer<ItemType>.Default.Equals(target, reference.Target)
            || obj is ItemType item && EqualityComparer<ItemType>.Default.Equals(target, item)
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

    public void Unset()
    {
        targetIndex = -1;
        target = default;
    }
}