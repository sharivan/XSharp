using System;
using System.Reflection;

using XSharp.Factories;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Serialization;

public abstract class Serializer : ISerializer
{
    public abstract void SerializeObject(string name, object obj);

    public abstract object DeserializeObject(string name);

    public abstract object DeserializeObject(Type type, string name);

    public void SerializeField(string name, Type type, object instance)
    {
        var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        var value = field.GetValue(instance);
        SerializeObject(name, value);
    }

    public void SerializeProperty(string name, Type type, object instance)
    {
        var field = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        var value = field.GetValue(instance);
        SerializeObject(name, value);
    }

    public void DeserializeField(string name, Type type, object instance)
    {
        var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        DeserializeField(field, type, instance);
    }

    public void DeserializeField(FieldInfo field, Type type, object instance)
    {
        string name = field.Name;
        var fieldType = field.FieldType;
        var value = DeserializeObject(fieldType, name);
        OnFieldDeserialized(field, instance, name, value);
    }

    public void DeserializeProperty(string name, Type type, object instance)
    {
        var property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        DeserializeProperty(property, instance);
    }

    public void DeserializeProperty(PropertyInfo property, object instance)
    {
        string name = property.Name;
        var propertyType = property.PropertyType;
        var value = DeserializeObject(propertyType, name);
        OnPropertyDeserialized(property, instance, name, value);
    }

    protected virtual void OnFieldDeserialized(FieldInfo field, object instance, string name, object value)
    {
        field.SetValue(instance, value);
    }

    protected virtual void OnPropertyDeserialized(PropertyInfo property, object instance, string name, object value)
    {
        property.SetValue(instance, value);
    }

    public abstract byte ReadByte();

    public abstract sbyte ReadSByte();

    public abstract short ReadShort();

    public abstract ushort ReadUShort();

    public abstract int ReadInt();

    public abstract uint ReadUInt();

    public abstract long ReadLong();

    public abstract ulong ReadULong();

    public abstract float ReadFloat();

    public abstract double ReadDouble();

    public abstract bool ReadBool();

    public abstract char ReadChar();

    public abstract string ReadString(bool nullable = true);

    public abstract FixedSingle ReadFixedSingle();

    public abstract FixedDouble ReadFixedDouble();

    public abstract Interval ReadInterval();

    public abstract Vector ReadVector();

    public abstract LineSegment ReadLineSegment();

    public abstract Box ReadBox();

    public abstract RightTriangle ReadRightTriangle();

    public abstract T ReadEnum<T>() where T : Enum;

    public abstract IFactoryItemReference ReadItemReference(Type referenceType, bool nullable = true);

    public abstract object ReadDelegate(bool nullable = true);

    public abstract object ReadObject(bool acceptNonSerializable = false, bool ignoreItems = false, bool nullable = true);

    public abstract void WriteByte(byte value);

    public abstract void WriteSByte(sbyte value);

    public abstract void WriteShort(short value);

    public abstract void WriteUShort(ushort value);

    public abstract void WriteInt(int value);

    public abstract void WriteUInt(uint value);

    public abstract void WriteLong(long value);

    public abstract void WriteULong(ulong value);

    public abstract void WriteFloat(float value);

    public abstract void WriteDouble(double value);

    public abstract void WriteBool(bool value);

    public abstract void WriteChar(char value);

    public abstract void WriteString(string value, bool nullable = true);

    public abstract void WriteFixedSingle(FixedSingle value);

    public abstract void WriteFixedDouble(FixedDouble value);

    public abstract void WriteInterval(Interval value);

    public abstract void WriteVector(Vector value);

    public abstract void WriteLineSegment(LineSegment value);

    public abstract void WriteBox(Box value);

    public abstract void WriteRightTriangle(RightTriangle value);

    public abstract void WriteEnum<T>(T value) where T : Enum;

    public abstract void WriteItemReference(IFactoryItemReference reference, bool nullable = true);

    public abstract void WriteDelegate(Delegate @delegate, bool nullable = true);

    public abstract void WriteObject(object obj, bool acceptNonSerializable = false, bool ignoreItems = false, bool nullable = true);
}