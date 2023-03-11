namespace XSharp.Factories;

public interface IIndexedFactoryItem : IFactoryItem
{
    new IIndexedFactory Factory
    {
        get;
    }

    IFactory IFactoryItem.Factory => Factory;

    int Index
    {
        get;
    }
}