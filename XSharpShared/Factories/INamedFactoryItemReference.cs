namespace XSharp.Factories;

public interface INamedFactoryItemReference : IFactoryItemReference
{
    new INamedFactory Factory
    {
        get;
    }

    IFactory IFactoryItemReference.Factory => Factory;

    new public INamedFactoryItem Target => GetTargetFromName();

    IFactoryItem IFactoryItemReference.Target => Target;

    public string TargetName
    {
        get;
    }

    INamedFactoryItem GetTargetFromName()
    {
        string targetName = TargetName;
        return targetName != null ? Factory[targetName] : default;
    }
}

public interface INamedFactoryItemReference<ItemType> : IFactoryItemReference<ItemType>, INamedFactoryItemReference where ItemType : INamedFactoryItem
{
    new INamedFactory<ItemType> Factory
    {
        get;
    }

    IFactory<ItemType> IFactoryItemReference<ItemType>.Factory => Factory;

    INamedFactory INamedFactoryItemReference.Factory => Factory;

    IFactory IFactoryItemReference.Factory => Factory;

    new public ItemType Target => GetTargetFromName();

    ItemType IFactoryItemReference<ItemType>.Target => Target;

    INamedFactoryItem INamedFactoryItemReference.Target => Target;

    IFactoryItem IFactoryItemReference.Target => Target;

    new ItemType GetTargetFromName()
    {
        string targetName = TargetName;
        return targetName != null ? Factory[targetName] : default;
    }
}