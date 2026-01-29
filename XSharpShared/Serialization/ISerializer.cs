using System;

using XSharp.Factories;
using XSharp.Math.Fixed;
using XSharp.Math.Fixed.Geometry;

namespace XSharp.Serialization;

public interface ISerializer
{
    byte ReadByte();

    sbyte ReadSByte();

    short ReadShort();

    ushort ReadUShort();

    int ReadInt();

    uint ReadUInt();

    long ReadLong();

    ulong ReadULong();

    float ReadFloat();

    double ReadDouble();

    bool ReadBool();

    char ReadChar();

    string ReadString(bool nullable = true);

    FixedSingle ReadFixedSingle();

    FixedDouble ReadFixedDouble();

    Interval ReadInterval();

    Vector ReadVector();

    LineSegment ReadLineSegment();

    Box ReadBox();

    RightTriangle ReadRightTriangle();

    T ReadEnum<T>() where T : Enum;

    IFactoryItemReference ReadItemReference(Type referenceType, bool nullable = true);

    object ReadDelegate(bool nullable = true);

    object ReadObject(bool acceptNonSerializable = false, bool ignoreItems = false, bool nullable = true);

    public ReferenceType ReadItemReference<ReferenceType>(bool nullable = true) where ReferenceType : IFactoryItemReference
    {
        return (ReferenceType) ReadItemReference(typeof(ReferenceType), nullable);
    }

    void WriteByte(byte value);

    void WriteSByte(sbyte value);

    void WriteShort(short value);

    void WriteUShort(ushort value);

    void WriteInt(int value);

    void WriteUInt(uint value);

    void WriteLong(long value);

    void WriteULong(ulong value);

    void WriteFloat(float value);

    void WriteDouble(double value);

    void WriteBool(bool value);

    void WriteChar(char value);

    void WriteString(string value, bool nullable = true);

    void WriteFixedSingle(FixedSingle value);

    void WriteFixedDouble(FixedDouble value);

    void WriteInterval(Interval value);

    void WriteVector(Vector value);

    void WriteLineSegment(LineSegment value);

    void WriteBox(Box value);

    void WriteRightTriangle(RightTriangle value);

    void WriteEnum<T>(T value) where T : Enum;

    void WriteItemReference(IFactoryItemReference reference, bool nullable = true);

    void WriteDelegate(Delegate @delegate, bool nullable = true);

    void WriteObject(object obj, bool acceptNonSerializable = false, bool ignoreItems = false, bool nullable = true);

    void SerializeField(string name, Type type, object instance);

    void SerializeProperty(string name, Type type, object instance);

    void DeserializeField(string name, Type type, object instance);

    void DeserializeProperty(string name, Type type, object instance);
}