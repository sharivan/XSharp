namespace XSharp.Factories;

public interface IIndexedNamedFactoryItem : IIndexedFactoryItem, INamedFactoryItem
{
    new IIndexedNamedFactory Factory
    {
        get;
    }

    IIndexedFactory IIndexedFactoryItem.Factory => Factory;

    INamedFactory INamedFactoryItem.Factory => Factory;

    IFactory IFactoryItem.Factory => Factory;
}