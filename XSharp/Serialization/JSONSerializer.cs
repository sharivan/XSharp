using System;
using System.Text.Json;

namespace XSharp.Serialization;

// TODO : Implement the remaining.
public class JSONSerializer : Serializer
{
    private JsonDocument document;

    public JSONSerializer(JsonDocument document)
    {
        this.document = document;
    }

    public override object DeserializeObject(string name)
    {
        return null;
    }

    public override object DeserializeObject(Type type, string name)
    {
        return null;
    }

    public T DeserializeObject<T>(string name)
    {
        return (T) JsonSerializer.Deserialize(document, typeof(T));
    }

    public override void SerializeObject(string name, object obj)
    {
        JsonSerializer.Serialize(obj);
    }
}