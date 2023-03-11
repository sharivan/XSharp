namespace XSharp.Serialization;

public interface ISerializable
{
    public void Deserialize(BinarySerializer serializer);

    public void Serialize(BinarySerializer serializer);
}