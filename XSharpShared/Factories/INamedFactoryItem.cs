namespace XSharp.Factories;

public interface INamedFactoryItem : IFactoryItem
{
    new INamedFactory Factory
    {
        get;
    }

    IFactory IFactoryItem.Factory => Factory;

    string Name
    {
        get;
    }
}