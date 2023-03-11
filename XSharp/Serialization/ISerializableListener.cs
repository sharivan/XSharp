namespace XSharp.Serialization;

public interface ISerializableListener
{
    void OnSerialized(BinarySerializer serializer);

    void OnDeserialized(BinarySerializer serializer);
}