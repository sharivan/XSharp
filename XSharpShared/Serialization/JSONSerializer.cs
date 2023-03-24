using System;
using System.Text.Json;

using XSharp.Factories;
using XSharp.Math;
using XSharp.Math.Geometry;

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

    public override bool ReadBool()
    {
        throw new NotImplementedException();
    }

    public override Box ReadBox()
    {
        throw new NotImplementedException();
    }

    public override byte ReadByte()
    {
        throw new NotImplementedException();
    }

    public override char ReadChar()
    {
        throw new NotImplementedException();
    }

    public override object ReadDelegate(bool nullable = true)
    {
        throw new NotImplementedException();
    }

    public override double ReadDouble()
    {
        throw new NotImplementedException();
    }

    public override T ReadEnum<T>()
    {
        throw new NotImplementedException();
    }

    public override FixedDouble ReadFixedDouble()
    {
        throw new NotImplementedException();
    }

    public override FixedSingle ReadFixedSingle()
    {
        throw new NotImplementedException();
    }

    public override float ReadFloat()
    {
        throw new NotImplementedException();
    }

    public override int ReadInt()
    {
        throw new NotImplementedException();
    }

    public override Interval ReadInterval()
    {
        throw new NotImplementedException();
    }

    public override IFactoryItemReference ReadItemReference(Type referenceType, bool nullable = true)
    {
        throw new NotImplementedException();
    }

    public override LineSegment ReadLineSegment()
    {
        throw new NotImplementedException();
    }

    public override long ReadLong()
    {
        throw new NotImplementedException();
    }

    public override object ReadObject(bool acceptNonSerializable = false, bool ignoreItems = false, bool nullable = true)
    {
        throw new NotImplementedException();
    }

    public override RightTriangle ReadRightTriangle()
    {
        throw new NotImplementedException();
    }

    public override sbyte ReadSByte()
    {
        throw new NotImplementedException();
    }

    public override short ReadShort()
    {
        throw new NotImplementedException();
    }

    public override string ReadString(bool nullable = true)
    {
        throw new NotImplementedException();
    }

    public override uint ReadUInt()
    {
        throw new NotImplementedException();
    }

    public override ulong ReadULong()
    {
        throw new NotImplementedException();
    }

    public override ushort ReadUShort()
    {
        throw new NotImplementedException();
    }

    public override Vector ReadVector()
    {
        throw new NotImplementedException();
    }

    public override void SerializeObject(string name, object obj)
    {
        JsonSerializer.Serialize(obj);
    }

    public override void WriteBool(bool value)
    {
        throw new NotImplementedException();
    }

    public override void WriteBox(Box value)
    {
        throw new NotImplementedException();
    }

    public override void WriteByte(byte value)
    {
        throw new NotImplementedException();
    }

    public override void WriteChar(char value)
    {
        throw new NotImplementedException();
    }

    public override void WriteDelegate(Delegate @delegate, bool nullable = true)
    {
        throw new NotImplementedException();
    }

    public override void WriteDouble(double value)
    {
        throw new NotImplementedException();
    }

    public override void WriteEnum<T>(T value)
    {
        throw new NotImplementedException();
    }

    public override void WriteFixedDouble(FixedDouble value)
    {
        throw new NotImplementedException();
    }

    public override void WriteFixedSingle(FixedSingle value)
    {
        throw new NotImplementedException();
    }

    public override void WriteFloat(float value)
    {
        throw new NotImplementedException();
    }

    public override void WriteInt(int value)
    {
        throw new NotImplementedException();
    }

    public override void WriteInterval(Interval value)
    {
        throw new NotImplementedException();
    }

    public override void WriteItemReference(IFactoryItemReference reference, bool nullable = true)
    {
        throw new NotImplementedException();
    }

    public override void WriteLineSegment(LineSegment value)
    {
        throw new NotImplementedException();
    }

    public override void WriteLong(long value)
    {
        throw new NotImplementedException();
    }

    public override void WriteObject(object obj, bool acceptNonSerializable = false, bool ignoreItems = false, bool nullable = true)
    {
        throw new NotImplementedException();
    }

    public override void WriteRightTriangle(RightTriangle value)
    {
        throw new NotImplementedException();
    }

    public override void WriteSByte(sbyte value)
    {
        throw new NotImplementedException();
    }

    public override void WriteShort(short value)
    {
        throw new NotImplementedException();
    }

    public override void WriteString(string value, bool nullable = true)
    {
        throw new NotImplementedException();
    }

    public override void WriteUInt(uint value)
    {
        throw new NotImplementedException();
    }

    public override void WriteULong(ulong value)
    {
        throw new NotImplementedException();
    }

    public override void WriteUShort(ushort value)
    {
        throw new NotImplementedException();
    }

    public override void WriteVector(Vector value)
    {
        throw new NotImplementedException();
    }
}