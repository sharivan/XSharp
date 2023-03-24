using System;
using System.Collections;
using System.Collections.Generic;

using XSharp.Serialization;

namespace XSharp.Factories;

public abstract class IndexedNamedFactory<ItemType> : IIndexedNamedFactory<ItemType>, ISerializable where ItemType : IIndexedNamedFactoryItem
{
    protected Dictionary<string, int> itemsByName;

    public abstract int Count
    {
        get;
    }

    public abstract IReadOnlyList<ItemType> Items
    {
        get;
    }

    public abstract IReadOnlyList<IndexedNamedFactoryItemReference<ItemType>> References
    {
        get;
    }

    IIndexedFactoryItem IIndexedFactory.this[int index] => this[index];

    INamedFactoryItem INamedFactory.this[string name] => this[name];

    public ItemType this[int index] => GetItemByIndex(index);

    public ItemType this[string name] => GetItemByName(name);

    public IndexedNamedFactory()
    {
        itemsByName = new Dictionary<string, int>();
    }

    protected abstract Type GetDefaultItemReferenceType(Type itemType);

    public ItemType GetItemByIndex(int index)
    {
        return index < 0 || index >= Items.Count ? default : Items[index];
    }

    public IndexedNamedFactoryItemReference<ItemType> GetItemReferenceByIndex(int index)
    {
        if (index < 0 || index >= Items.Count)
            return null;

        var item = Items[index];
        return item != null ? References[index] : null;
    }

    public ItemType GetItemByName(string name)
    {
        return itemsByName.TryGetValue(name, out int index) ? GetItemByIndex(index) : default;
    }

    public IndexedNamedFactoryItemReference<ItemType> GetItemReferenceByName(string name)
    {
        return itemsByName.TryGetValue(name, out int index) ? GetItemReferenceByIndex(index) : null;
    }

    public IIndexedFactoryItemReference<ItemType> GetReferenceTo(int index)
    {
        return GetItemReferenceByIndex(index);
    }

    public INamedFactoryItemReference<ItemType> GetReferenceTo(string name)
    {
        return GetItemReferenceByName(name);
    }

    IIndexedNamedFactoryItemReference<ItemType> IIndexedNamedFactory<ItemType>.GetReferenceTo(int index)
    {
        return GetItemReferenceByIndex(index);
    }

    IIndexedNamedFactoryItemReference<ItemType> IIndexedNamedFactory<ItemType>.GetReferenceTo(string name)
    {
        return GetItemReferenceByName(name);
    }

    public IndexedNamedFactoryItemReference<ItemType> GetReferenceTo(ItemType item)
    {
        return item != null ? GetItemReferenceByIndex(item.Index) : null;
    }

    public virtual IEnumerator<ItemType> GetEnumerator()
    {
        return Items.GetEnumerator();
    }

    protected abstract void SetReferenceTo(int index, IndexedNamedFactoryItemReference<ItemType> reference);

    protected abstract void SetReferenceTo(string name, IndexedNamedFactoryItemReference<ItemType> reference);

    void IIndexedNamedFactory.SetReferenceTo(int index, IIndexedNamedFactoryItemReference reference)
    {
        SetReferenceTo(index, (IndexedNamedFactoryItemReference<ItemType>) reference);
    }

    void IIndexedNamedFactory.SetReferenceTo(string name, IIndexedNamedFactoryItemReference reference)
    {
        SetReferenceTo(name, (IndexedNamedFactoryItemReference<ItemType>) reference);
    }

    IIndexedFactoryItemReference IIndexedFactory.GetReferenceTo(IIndexedFactoryItem item)
    {
        return GetReferenceTo((ItemType) item);
    }

    INamedFactoryItemReference INamedFactory.GetReferenceTo(string name)
    {
        return GetReferenceTo(name);
    }

    INamedFactoryItemReference INamedFactory.GetReferenceTo(INamedFactoryItem item)
    {
        return GetReferenceTo((ItemType) item);
    }

    IFactoryItemReference IFactory.GetReferenceTo(IFactoryItem item)
    {
        return GetReferenceTo((ItemType) item);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator<ItemType> IEnumerable<ItemType>.GetEnumerator()
    {
        return GetEnumerator();
    }

    protected virtual DuplicateItemNameException<ItemType> CreateDuplicateNameException(string name)
    {
        return new DuplicateItemNameException<ItemType>(name);
    }

    protected abstract void SetItemFactory(ItemType item, IndexedNamedFactoryItemReference<ItemType> reference);

    protected abstract void SetItemIndex(ItemType item, IndexedNamedFactoryItemReference<ItemType> reference, int index);

    protected abstract void SetItemName(ItemType item, IndexedNamedFactoryItemReference<ItemType> reference, string name);

    protected virtual bool CanChangeName(ItemType item)
    {
        return true;
    }

    protected internal void UpdateItemName(ItemType item, string name)
    {
        if (name == item.Name)
            return;

        if (itemsByName.ContainsKey(name))
            throw CreateDuplicateNameException(name);

        if (CanChangeName(item))
        {
            if (item.Name is not null and not "")
                itemsByName.Remove(item.Name);

            itemsByName.Add(name, item.Index);
        }

        var reference = References[item.Index];
        SetItemName(item, reference, name);
    }

    public string GetExclusiveName(string prefix, bool startWithCounterSuffix = false, int startCounterSuffix = 1)
    {
        int counter = startCounterSuffix;
        string possibleName = startWithCounterSuffix ? prefix + counter++ : prefix;

        for (ItemType entity = this[prefix]; entity != null; entity = this[possibleName])
            possibleName = prefix + counter++;

        return possibleName;
    }

    public virtual IndexedNamedFactoryItemReference<ItemType> GetOrCreateReferenceTo(int index, Type fallBackItemType, bool forceUseFallBackType = false)
    {
        if (index < 0)
            return null;

        IndexedNamedFactoryItemReference<ItemType> reference = GetItemReferenceByIndex(index);
        if (reference is not null)
            return reference;

        ItemType item;

        Type itemType = !forceUseFallBackType && (item = Items[index]) != null ? item.GetType() : fallBackItemType;
        Type referenceType = GetDefaultItemReferenceType(itemType);

        reference = (IndexedNamedFactoryItemReference<ItemType>) Activator.CreateInstance(referenceType);
        UpdateReferenceIndex(reference, index);
        SetReferenceTo(index, reference);

        return reference;
    }

    public virtual IndexedNamedFactoryItemReference<ItemType> GetOrCreateReferenceTo(string name, Type fallBackItemType, bool forceUseFallBackType = false)
    {
        if (name == null)
            return null;

        IndexedNamedFactoryItemReference<ItemType> reference = GetItemReferenceByName(name);
        if (reference is not null)
            return reference;

        ItemType item;

        Type itemType = !forceUseFallBackType && (item = GetItemByName(name)) != null ? item.GetType() : fallBackItemType;
        Type referenceType = GetDefaultItemReferenceType(itemType);

        reference = (IndexedNamedFactoryItemReference<ItemType>) Activator.CreateInstance(referenceType);
        UpdateReferenceName(reference, name);
        SetReferenceTo(name, reference);

        return reference;
    }

    public virtual void Deserialize(ISerializer serializer)
    {
        if (itemsByName == null)
            itemsByName = new Dictionary<string, int>();
        else
            itemsByName.Clear();

        int count = serializer.ReadInt();
        for (int i = 0; i < count; i++)
        {
            string name = serializer.ReadString();
            int index = serializer.ReadInt();
            itemsByName.Add(name, index);
        }
    }

    public virtual void Serialize(ISerializer serializer)
    {
        serializer.WriteInt(itemsByName.Count);
        foreach (var (name, index) in itemsByName)
        {
            serializer.WriteString(name);
            serializer.WriteInt(index);
        }
    }

    public void UpdateReferenceIndex(IIndexedFactoryItemReference reference, int index)
    {
        ((IndexedNamedFactoryItemReference<ItemType>) reference).TargetIndex = index;
    }

    public void UpdateReferenceName(INamedFactoryItemReference reference, string name)
    {
        ((IndexedNamedFactoryItemReference<ItemType>) reference).TargetName = name;
    }
}