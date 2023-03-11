namespace XSharp.Factories;

public interface INamedFactory : IFactory
{
    INamedFactoryItem this[string name]
    {
        get;
    }

    INamedFactoryItemReference GetReferenceTo(string name);

    void SetReferenceTo(string name, INamedFactoryItemReference reference);

    public INamedFactoryItemReference GetOrCreateReferenceTo<ReferenceType>(string name) where ReferenceType : class, INamedFactoryItemReference, new()
    {
        if (name == null)
            return null;

        INamedFactoryItemReference reference = GetReferenceTo(name);
        if (reference is not null)
            return reference;

        reference = new ReferenceType();
        UpdateReferenceName(reference, name);

        return reference;
    }

    INamedFactoryItemReference GetReferenceTo(INamedFactoryItem item);

    IFactoryItemReference IFactory.GetReferenceTo(IFactoryItem item)
    {
        return GetReferenceTo((INamedFactoryItem) item);
    }

    void UpdateReferenceName(INamedFactoryItemReference reference, string name);
}

public interface INamedFactory<ItemType> : IFactory<ItemType>, INamedFactory where ItemType : INamedFactoryItem
{
    new ItemType this[string name]
    {
        get;
    }

    INamedFactoryItem INamedFactory.this[string name] => this[name];

    new INamedFactoryItemReference<ItemType> GetReferenceTo(string name);

    INamedFactoryItemReference INamedFactory.GetReferenceTo(string name)
    {
        return GetReferenceTo(name);
    }

    new INamedFactoryItemReference<ItemType> GetReferenceTo(INamedFactoryItem item);

    INamedFactoryItemReference INamedFactory.GetReferenceTo(INamedFactoryItem item)
    {
        return GetReferenceTo(item);
    }

    IFactoryItemReference<ItemType> IFactory<ItemType>.GetReferenceTo(IFactoryItem item)
    {
        return GetReferenceTo((INamedFactoryItem) item);
    }
}