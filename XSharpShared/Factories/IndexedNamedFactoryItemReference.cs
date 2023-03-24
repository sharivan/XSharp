using System.Collections.Generic;

using XSharp.Serialization;

namespace XSharp.Factories;

public abstract class IndexedNamedFactoryItemReference<ItemType> : IIndexedNamedFactoryItemReference<ItemType> where ItemType : IIndexedNamedFactoryItem
{
    protected string targetName = null;
    protected int targetIndex = -1;

    protected ItemType target = default;

    public IndexedNamedFactory<ItemType> Factory => GetFactory();

    IIndexedNamedFactory<ItemType> IIndexedNamedFactoryItemReference<ItemType>.Factory => GetFactory();

    public virtual ItemType Target
    {
        get
        {
            target ??= ((IIndexedNamedFactoryItemReference<ItemType>) this).GetTargetFromIndexOrName();
            return target;
        }
    }

    public virtual int TargetIndex
    {
        get => targetIndex;

        set
        {
            targetIndex = value;
            targetName = null;
            target = default;
        }
    }

    public virtual string TargetName
    {
        get => targetName;

        set
        {
            targetName = value;
            targetIndex = -1;
            target = default;
        }
    }

    protected IndexedNamedFactoryItemReference()
    {
    }

    public virtual void Deserialize(ISerializer reader)
    {
        targetIndex = reader.ReadInt();
        targetName = reader.ReadString();
        target = default;
    }

    public virtual void Serialize(ISerializer writer)
    {
        writer.WriteInt(TargetIndex);
        writer.WriteString(TargetName);
    }

    protected abstract IndexedNamedFactory<ItemType> GetFactory();

    public static implicit operator ItemType(IndexedNamedFactoryItemReference<ItemType> reference)
    {
        return reference != null ? reference.Target : default;
    }

    public static bool operator ==(IndexedNamedFactoryItemReference<ItemType> reference1, IndexedNamedFactoryItemReference<ItemType> reference2)
    {
        return ReferenceEquals(reference1, reference2)
            || reference1 is not null && reference2 is not null && reference1.Equals(reference2);
    }

    public static bool operator ==(IndexedNamedFactoryItemReference<ItemType> reference, ItemType item)
    {
        return reference is null ? item is null : EqualityComparer<ItemType>.Default.Equals(reference.Target, item);
    }

    public static bool operator ==(ItemType item, IndexedNamedFactoryItemReference<ItemType> reference)
    {
        return reference == item;
    }

    public static bool operator !=(IndexedNamedFactoryItemReference<ItemType> reference1, IndexedNamedFactoryItemReference<ItemType> reference2)
    {
        return !(reference1 == reference2);
    }

    public static bool operator !=(IndexedNamedFactoryItemReference<ItemType> reference, ItemType item)
    {
        return !(reference == item);
    }

    public static bool operator !=(ItemType item, IndexedNamedFactoryItemReference<ItemType> reference)
    {
        return !(item == reference);
    }

    public override bool Equals(object? obj)
    {
        var target = Target;
        return ReferenceEquals(obj, target)
            || obj != null
            && (
            obj is IndexedNamedFactoryItemReference<ItemType> reference && EqualityComparer<ItemType>.Default.Equals(target, reference.Target)
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

    public virtual void UpdateTargetNameFromIndex()
    {
        target = ((IIndexedFactoryItemReference<ItemType>) this).GetTargetFromIndex();
        targetName = target?.Name;
    }

    public virtual void UpdateTargetIndexFromName()
    {
        target = ((INamedFactoryItemReference<ItemType>) this).GetTargetFromName();
        targetIndex = target != null ? target.Index : -1;
    }

    public virtual void Unset()
    {
        target = default;
        targetIndex = -1;
        targetName = null;
    }
}