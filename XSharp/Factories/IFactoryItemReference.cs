using System;

using XSharp.Serialization;

namespace XSharp.Factories;

public interface IFactoryItemReference : ISerializable
{
    public IFactory Factory
    {
        get;
    }

    public IFactoryItem Target
    {
        get;
    }

    Type ItemDefaultType
    {
        get;
    }
}

public interface IFactoryItemReference<ItemType> : IFactoryItemReference where ItemType : IFactoryItem
{
    new public IFactory<ItemType> Factory
    {
        get;
    }

    IFactory IFactoryItemReference.Factory => Factory;

    new public ItemType Target
    {
        get;
    }

    IFactoryItem IFactoryItemReference.Target => Target;

    Type IFactoryItemReference.ItemDefaultType => typeof(ItemType);

    void Unset();
}