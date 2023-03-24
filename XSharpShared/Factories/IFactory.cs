using System.Collections;
using System.Collections.Generic;

namespace XSharp.Factories;

public interface IFactory : IEnumerable
{
    IFactoryItemReference GetReferenceTo(IFactoryItem item);
}

public interface IFactory<ItemType> : IEnumerable<ItemType>, IFactory where ItemType : IFactoryItem
{
    new IFactoryItemReference<ItemType> GetReferenceTo(IFactoryItem item);

    IFactoryItemReference IFactory.GetReferenceTo(IFactoryItem item)
    {
        return GetReferenceTo(item);
    }
}