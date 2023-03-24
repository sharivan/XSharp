namespace XSharp.Factories;

public interface IIndexedNamedFactory : IIndexedFactory, INamedFactory
{
    new IIndexedNamedFactoryItem this[int index]
    {
        get;
    }

    new IIndexedNamedFactoryItem this[string name]
    {
        get;
    }

    IIndexedFactoryItem IIndexedFactory.this[int index] => this[index];

    INamedFactoryItem INamedFactory.this[string name] => this[name];

    public IIndexedNamedFactoryItemReference GetReferenceTo(IIndexedNamedFactoryItem item)
    {
        string name = item.Name;
        if (name != null)
        {
            IIndexedNamedFactoryItemReference reference = GetReferenceTo(name);
            if (reference != null)
                return reference;
        }

        int index = item.Index;
        return GetReferenceTo(index);
    }

    new IIndexedNamedFactoryItemReference GetReferenceTo(int index);

    void SetReferenceTo(int index, IIndexedNamedFactoryItemReference reference);

    IIndexedFactoryItemReference IIndexedFactory.GetReferenceTo(int index)
    {
        return GetReferenceTo(index);
    }

    void IIndexedFactory.SetReferenceTo(int index, IIndexedFactoryItemReference reference)
    {
        SetReferenceTo(index, (IIndexedNamedFactoryItemReference) reference);
    }

    new IIndexedNamedFactoryItemReference GetReferenceTo(string name);

    void SetReferenceTo(string name, IIndexedNamedFactoryItemReference reference);

    INamedFactoryItemReference INamedFactory.GetReferenceTo(string name)
    {
        return GetReferenceTo(name);
    }

    void INamedFactory.SetReferenceTo(string name, INamedFactoryItemReference reference)
    {
        SetReferenceTo(name, (IIndexedNamedFactoryItemReference) reference);
    }

    IFactoryItemReference IFactory.GetReferenceTo(IFactoryItem item)
    {
        return GetReferenceTo((IIndexedNamedFactoryItem) item);
    }

    IIndexedFactoryItemReference IIndexedFactory.GetReferenceTo(IIndexedFactoryItem item)
    {
        return GetReferenceTo((IIndexedNamedFactoryItem) item);
    }

    INamedFactoryItemReference INamedFactory.GetReferenceTo(INamedFactoryItem item)
    {
        return GetReferenceTo((IIndexedNamedFactoryItem) item);
    }
}

public interface IIndexedNamedFactory<ItemType> : IIndexedFactory<ItemType>, INamedFactory<ItemType>, IIndexedNamedFactory where ItemType : IIndexedNamedFactoryItem
{
    new ItemType this[int index]
    {
        get;
    }

    new ItemType this[string name]
    {
        get;
    }

    ItemType IIndexedFactory<ItemType>.this[int index] => this[index];

    ItemType INamedFactory<ItemType>.this[string name] => this[name];

    IIndexedNamedFactoryItem IIndexedNamedFactory.this[int index] => this[index];

    IIndexedNamedFactoryItem IIndexedNamedFactory.this[string name] => this[name];

    new public IIndexedNamedFactoryItemReference<ItemType> GetReferenceTo(IIndexedNamedFactoryItem item)
    {
        string name = item.Name;
        if (name != null)
        {
            IIndexedNamedFactoryItemReference<ItemType> reference = GetReferenceTo(name);
            if (reference != null)
                return reference;
        }

        int index = item.Index;
        return GetReferenceTo(index);
    }

    new IIndexedNamedFactoryItemReference<ItemType> GetReferenceTo(int index);

    IIndexedNamedFactoryItemReference IIndexedNamedFactory.GetReferenceTo(int index)
    {
        return GetReferenceTo(index);
    }

    IIndexedFactoryItemReference<ItemType> IIndexedFactory<ItemType>.GetReferenceTo(int index)
    {
        return GetReferenceTo(index);
    }

    new IIndexedNamedFactoryItemReference<ItemType> GetReferenceTo(string name);

    IIndexedNamedFactoryItemReference IIndexedNamedFactory.GetReferenceTo(string name)
    {
        return GetReferenceTo(name);
    }

    INamedFactoryItemReference<ItemType> INamedFactory<ItemType>.GetReferenceTo(string name)
    {
        return GetReferenceTo(name);
    }

    IFactoryItemReference<ItemType> IFactory<ItemType>.GetReferenceTo(IFactoryItem item)
    {
        return GetReferenceTo((IIndexedNamedFactoryItem) item);
    }

    IIndexedFactoryItemReference<ItemType> IIndexedFactory<ItemType>.GetReferenceTo(IIndexedFactoryItem item)
    {
        return GetReferenceTo((IIndexedNamedFactoryItem) item);
    }

    INamedFactoryItemReference<ItemType> INamedFactory<ItemType>.GetReferenceTo(INamedFactoryItem item)
    {
        return GetReferenceTo((IIndexedNamedFactoryItem) item);
    }
}