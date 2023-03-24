namespace XSharp.Factories;

public interface IIndexedFactoryItemReference : IFactoryItemReference
{
    new IIndexedFactory Factory
    {
        get;
    }

    IFactory IFactoryItemReference.Factory => Factory;

    new public IIndexedFactoryItem Target => GetTargetFromIndex();

    IFactoryItem IFactoryItemReference.Target => Target;

    public int TargetIndex
    {
        get;
    }

    IIndexedFactoryItem GetTargetFromIndex()
    {
        int targetIndex = TargetIndex;
        return targetIndex >= 0 ? Factory[targetIndex] : default;
    }
}

public interface IIndexedFactoryItemReference<ItemType> : IFactoryItemReference<ItemType>, IIndexedFactoryItemReference where ItemType : IIndexedFactoryItem
{
    new public IIndexedFactory<ItemType> Factory
    {
        get;
    }

    IFactory<ItemType> IFactoryItemReference<ItemType>.Factory => Factory;

    IIndexedFactory IIndexedFactoryItemReference.Factory => Factory;

    IFactory IFactoryItemReference.Factory => Factory;

    new public ItemType Target => GetTargetFromIndex();

    ItemType IFactoryItemReference<ItemType>.Target => Target;

    IIndexedFactoryItem IIndexedFactoryItemReference.Target => Target;

    IFactoryItem IFactoryItemReference.Target => Target;

    new ItemType GetTargetFromIndex()
    {
        int targetIndex = TargetIndex;
        return targetIndex >= 0 ? Factory[targetIndex] : default;
    }
}