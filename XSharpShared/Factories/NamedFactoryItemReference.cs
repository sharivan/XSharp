using System.Collections.Generic;

using XSharp.Serialization;

namespace XSharp.Factories;

public abstract class NamedFactoryItemReference<ItemType> : INamedFactoryItemReference<ItemType> where ItemType : INamedFactoryItem
{
    internal string targetName = null;
    internal ItemType target = default;

    //public NamedFactoryContainer<ItemType> Factory => GetFactory();

    public INamedFactory<ItemType> Factory => GetFactory();

    public virtual ItemType Target
    {
        get
        {
            target ??= ((INamedFactoryItemReference<ItemType>) this).GetTargetFromName();
            return target;
        }
    }

    public virtual string TargetName => targetName;

    protected NamedFactoryItemReference()
    {
    }

    protected abstract INamedFactory<ItemType> GetFactory();

    public virtual void Deserialize(ISerializer reader)
    {
        targetName = reader.ReadString();
    }

    public virtual void Serialize(ISerializer writer)
    {
        writer.WriteString(targetName);
    }

    public static implicit operator ItemType(NamedFactoryItemReference<ItemType> reference)
    {
        return reference != null ? reference.Target : default;
    }

    public static bool operator ==(NamedFactoryItemReference<ItemType> reference1, NamedFactoryItemReference<ItemType> reference2)
    {
        return ReferenceEquals(reference1, reference2)
            || reference1 is not null && reference2 is not null && reference1.Equals(reference2);
    }

    public static bool operator ==(NamedFactoryItemReference<ItemType> reference, ItemType item)
    {
        return reference is null ? item is null : EqualityComparer<ItemType>.Default.Equals(reference.Target, item);
    }

    public static bool operator ==(ItemType item, NamedFactoryItemReference<ItemType> reference)
    {
        return reference == item;
    }

    public static bool operator !=(NamedFactoryItemReference<ItemType> reference1, NamedFactoryItemReference<ItemType> reference2)
    {
        return !(reference1 == reference2);
    }

    public static bool operator !=(NamedFactoryItemReference<ItemType> reference, ItemType item)
    {
        return !(reference == item);
    }

    public static bool operator !=(ItemType item, NamedFactoryItemReference<ItemType> reference)
    {
        return !(item == reference);
    }

    public override bool Equals(object? obj)
    {
        var target = Target;
        return ReferenceEquals(obj, target)
            || obj != null
            && (
            obj is NamedFactoryItemReference<ItemType> reference && EqualityComparer<ItemType>.Default.Equals(target, reference.Target)
            || obj is ItemType item && EqualityComparer<ItemType>.Default.Equals(target, item)
            );
    }

    public override int GetHashCode()
    {
        var targetName = TargetName;
        return targetName != null ? targetName.GetHashCode() : 0;
    }

    public override string ToString()
    {
        var target = Target;
        return target != null ? target.ToString() : "null";
    }

    public void Unset()
    {
        targetName = null;
        target = default;
    }
}