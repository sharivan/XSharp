namespace XSharp.Serialization;

public interface ISerializableListener
{
    void OnSerialized(Serializer serializer);

    void OnDeserialized(Serializer serializer);
}