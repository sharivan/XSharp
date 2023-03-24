namespace XSharp.Serialization;

public interface ISerializable
{
    public void Deserialize(ISerializer serializer);

    public void Serialize(ISerializer serializer);
}