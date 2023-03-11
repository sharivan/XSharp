namespace XSharp.Factories;

public interface IIndexedFactory : IFactory
{
    IIndexedFactoryItem this[int index]
    {
        get;
    }

    IIndexedFactoryItemReference GetReferenceTo(int index);

    void SetReferenceTo(int index, IIndexedFactoryItemReference reference);

    public IIndexedFactoryItemReference GetOrCreateReferenceTo<ReferenceType>(int index) where ReferenceType : class, IIndexedFactoryItemReference, new()
    {
        if (index < 0)
            return null;

        IIndexedFactoryItemReference reference = GetReferenceTo(index);
        if (reference is not null)
            return reference;

        reference = new ReferenceType();
        UpdateReferenceIndex(reference, index);

        return reference;
    }

    IIndexedFactoryItemReference GetReferenceTo(IIndexedFactoryItem item);

    IFactoryItemReference IFactory.GetReferenceTo(IFactoryItem item)
    {
        return GetReferenceTo((IIndexedFactoryItem) item);
    }

    void UpdateReferenceIndex(IIndexedFactoryItemReference reference, int index);
}

public interface IIndexedFactory<ItemType> : IFactory<ItemType>, IIndexedFactory where ItemType : IIndexedFactoryItem
{
    new ItemType this[int index]
    {
        get;
    }

    IIndexedFactoryItem IIndexedFactory.this[int index] => this[index];

    new IIndexedFactoryItemReference<ItemType> GetReferenceTo(int index);

    new IIndexedFactoryItemReference<ItemType> GetReferenceTo(IIndexedFactoryItem item);

    IIndexedFactoryItemReference IIndexedFactory.GetReferenceTo(IIndexedFactoryItem item)
    {
        return GetReferenceTo(item);
    }

    IFactoryItemReference<ItemType> IFactory<ItemType>.GetReferenceTo(IFactoryItem item)
    {
        return GetReferenceTo((IIndexedFactoryItem) item);
    }
}