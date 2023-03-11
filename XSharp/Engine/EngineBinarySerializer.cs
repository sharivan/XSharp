using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

using XSharp.Factories;
using XSharp.Engine.Entities;
using XSharp.Engine.Graphics;
using XSharp.Math;
using XSharp.Math.Geometry;
using XSharp.Serialization;

namespace XSharp.Engine;

public class EngineBinarySerializer : BinarySerializer
{
    public EngineBinarySerializer(Stream stream) : base(stream)
    {
    }

    public EngineBinarySerializer(BinaryReader reader) : base(reader)
    {
    }

    public EngineBinarySerializer(BinaryWriter writer) : base(writer)
    {
    }

    public override IFactoryItemReference ReadItemReference(Type referenceType, bool nullable = true)
    {
        if (referenceType.IsGenericType && referenceType.GetGenericTypeDefinition() == typeof(EntityReference<>))
            return ReadEntityReference(referenceType.GetGenericArguments()[0], nullable);

        if (referenceType == typeof(EntityReference))
            return ReadEntityReference(typeof(Entity), nullable);

        return base.ReadItemReference(referenceType, nullable);
    }

    public EntityReference ReadEntityReference(Type entityType, bool nullable = false)
    {
        if (nullable)
        {
            bool isSet = ReadBool();
            if (!isSet)
                return null;
        }

        int index = ReadInt();
        var reference = GameEngine.Engine.Entities.GetOrCreateReferenceTo(index, entityType);
        if (reference is null)
            return null;

        var referenceType = entityType == typeof(Entity) ? typeof(EntityReference) : typeof(EntityReference<>).MakeGenericType(entityType);
        if (reference.GetType().IsAssignableTo(referenceType))
            return reference;

        var proxyReferenceType = typeof(EntityProxyReference<>).MakeGenericType(entityType);
        return (EntityReference) Activator.CreateInstance(proxyReferenceType, reference);
    }

    public EntityReference ReadEntityReference(bool nullable = false)
    {
        return ReadEntityReference(typeof(Entity), nullable);
    }

    public EntityReference<EntityType> ReadEntityReference<EntityType>(bool nullable = false) where EntityType : Entity
    {
        return (EntityReference<EntityType>) ReadEntityReference(typeof(EntityType), nullable);
    }

    public void WriteEntityReference(EntityReference value, bool nullable = false)
    {
        if (nullable)
        {
            if (value is null)
                WriteBool(false);
            else
            {
                WriteBool(true);
                writer.Write(value.TargetIndex);
            }
        }
        else
            writer.Write(value is not null ? value.TargetIndex : -1);
    }

    public override object ReadValue(Type type, bool acceptNonSerializable = false, bool ignoreItems = false, bool nullable = true)
    {
        if (type == typeof(GameEngine))
        {
            bool isSet = ReadBool();
            return isSet ? GameEngine.Engine : null;
        }

        if (type == typeof(World.World))
        {
            bool isSet = ReadBool();
            return isSet ? GameEngine.Engine.World : null;
        }

        if (type == typeof(Partition<Entity>))
        {
            bool isSet = ReadBool();
            return isSet ? GameEngine.Engine.partition : null;
        }

        return base.ReadValue(type, acceptNonSerializable, ignoreItems, nullable);
    }

    public override void WriteValue(Type type, object value, bool acceptNonSerializable = false, bool ignoreItems = false, bool nullable = true)
    {
        if (type == typeof(GameEngine) || type == typeof(World.World) || type == typeof(Partition<Entity>))
            WriteBool(value != null);
        else
            base.WriteValue(type, value, acceptNonSerializable, ignoreItems, nullable);
    }
}