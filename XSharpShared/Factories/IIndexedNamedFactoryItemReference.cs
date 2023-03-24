namespace XSharp.Factories;

public interface IIndexedNamedFactoryItemReference : IIndexedFactoryItemReference, INamedFactoryItemReference
{
    new IIndexedNamedFactory Factory
    {
        get;
    }

    IIndexedFactory IIndexedFactoryItemReference.Factory => Factory;

    INamedFactory INamedFactoryItemReference.Factory => Factory;

    IFactory IFactoryItemReference.Factory => Factory;

    new public IIndexedNamedFactoryItem Target => GetTargetFromIndexOrName();

    IIndexedFactoryItem IIndexedFactoryItemReference.Target => Target;

    INamedFactoryItem INamedFactoryItemReference.Target => Target;

    IFactoryItem IFactoryItemReference.Target => Target;

    IIndexedNamedFactoryItem GetTargetFromIndexOrName()
    {
        int targetIndex = TargetIndex;
        if (targetIndex >= 0)
        {
            IIndexedNamedFactoryItem target = Factory[targetIndex];
            UpdateTargetNameFromIndex();
            return target;
        }

        string targetName = TargetName;
        if (targetName != null)
        {
            IIndexedNamedFactoryItem target = Factory[targetName];
            UpdateTargetIndexFromName();
            return target;
        }

        return default;
    }

    void UpdateTargetNameFromIndex();

    void UpdateTargetIndexFromName();
}

public interface IIndexedNamedFactoryItemReference<ItemType> : IIndexedFactoryItemReference<ItemType>, INamedFactoryItemReference<ItemType>, IIndexedNamedFactoryItemReference where ItemType : IIndexedNamedFactoryItem
{
    new public IIndexedNamedFactory<ItemType> Factory
    {
        get;
    }

    IIndexedFactory<ItemType> IIndexedFactoryItemReference<ItemType>.Factory => Factory;

    INamedFactory<ItemType> INamedFactoryItemReference<ItemType>.Factory => Factory;

    IFactory<ItemType> IFactoryItemReference<ItemType>.Factory => Factory;

    IIndexedNamedFactory IIndexedNamedFactoryItemReference.Factory => Factory;

    IIndexedFactory IIndexedFactoryItemReference.Factory => Factory;

    INamedFactory INamedFactoryItemReference.Factory => Factory;

    IFactory IFactoryItemReference.Factory => Factory;

    new ItemType Target => GetTargetFromIndexOrName();

    ItemType IIndexedFactoryItemReference<ItemType>.Target => Target;

    ItemType INamedFactoryItemReference<ItemType>.Target => Target;

    ItemType IFactoryItemReference<ItemType>.Target => Target;

    IIndexedFactoryItem IIndexedFactoryItemReference.Target => Target;

    INamedFactoryItem INamedFactoryItemReference.Target => Target;

    IFactoryItem IFactoryItemReference.Target => Target;

    new ItemType GetTargetFromIndexOrName()
    {
        return (ItemType) ((IIndexedNamedFactoryItemReference) this).GetTargetFromIndexOrName();
    }
}