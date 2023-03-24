using XSharp.Math.Geometry;
using XSharp.Serialization;

namespace XSharp.Engine.Entities;

public record RespawnEntry : ISerializable
{
    public EntityReference Entity
    {
        get;
        internal set;
    }

    public Vector Origin
    {
        get;
        internal set;
    }

    public RespawnEntry(EntityReference entity, Vector origin)
    {
        Entity = entity;
        Origin = origin;
    }

    public void Deconstruct(out EntityReference entity, out Vector origin)
    {
        entity = Entity;
        origin = Origin;
    }

    public void Deserialize(ISerializer input)
    {
        var serializer = (EngineBinarySerializer) input;

        Entity = serializer.ReadEntityReference();
        Origin = serializer.ReadVector();
    }

    public void Serialize(ISerializer output)
    {
        var serializer = (EngineBinarySerializer) output;

        serializer.WriteEntityReference(Entity);
        serializer.WriteVector(Origin);
    }
}