using System.Collections.Generic;

using XSharp.Serialization;

namespace XSharp.Factories;

public abstract class IndexedNamedFactoryList<ItemType> : IndexedNamedFactory<ItemType> where ItemType : IIndexedNamedFactoryItem
{
    private List<ItemType> items;
    private List<IndexedNamedFactoryItemReference<ItemType>> references;

    public override int Count => items.Count;

    public override IReadOnlyList<ItemType> Items => items;

    public override IReadOnlyList<IndexedNamedFactoryItemReference<ItemType>> References => references;

    public IndexedNamedFactoryList()
    {
        items = new List<ItemType>();
        references = new List<IndexedNamedFactoryItemReference<ItemType>>();
    }

    protected abstract void OnCreateReference(IndexedNamedFactoryItemReference<ItemType> reference);

    public SubItemType Create<SubItemType, ReferenceType>()
        where SubItemType : ItemType, new()
        where ReferenceType : IndexedNamedFactoryItemReference<ItemType>, new()
    {
        var item = new SubItemType();

        int index = items.Count;

        items.Add(item);

        var reference = new ReferenceType
        {
            TargetIndex = item.Index
        };

        OnCreateReference(reference);
        references.Add(reference);

        SetItemFactory(item, reference);
        SetItemIndex(item, reference, index);

        return item;
    }

    public void Clear()
    {
        items.Clear();
        itemsByName.Clear();
        references.Clear();
    }

    public override void Deserialize(ISerializer serializer)
    {
        base.Deserialize(serializer);

        if (items == null)
            items = new List<ItemType>();
        else
            items.Clear();

        if (references == null)
            references = new List<IndexedNamedFactoryItemReference<ItemType>>();
        else
            references.Clear();

        int count = serializer.ReadInt();
        for (int i = 0; i < count; i++)
        {
            items.Add(default);
            references.Add(null);
        }

        for (int i = 0; i < count; i++)
        {
            var item = (ItemType) serializer.ReadObject(false, true);
            items[i] = item;
            GetOrCreateReferenceTo(i, typeof(ItemType));
        }
    }

    public override void Serialize(ISerializer serializer)
    {
        base.Serialize(serializer);

        serializer.WriteInt(items.Count);
        foreach (var item in items)
            serializer.WriteObject(item, false, true);
    }

    protected override void SetReferenceTo(int index, IndexedNamedFactoryItemReference<ItemType> reference)
    {
        references[index] = reference;
    }

    protected override void SetReferenceTo(string name, IndexedNamedFactoryItemReference<ItemType> reference)
    {
        if (itemsByName.TryGetValue(name, out int index))
            references[index] = reference;
    }
}