using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

using XSharp.Factories;
using XSharp.Math;
using XSharp.Math.Geometry;

using TupleExtensions = XSharp.Util.TupleExtensions;

namespace XSharp.Serialization;

public class BinarySerializer : Serializer, IDisposable
{
    public interface IFuture
    {
        object Value
        {
            get;
        }

        bool IsResolved
        {
            get;
        }

        bool Resolve();
    }

    public interface IFuture<T> : IFuture
    {
        new T Value
        {
            get;
        }

        object IFuture.Value => Value;
    }

    public class FutureItem : IFuture
    {
        internal IFactoryItemReference reference;
        internal IFactoryItem value = null;

        public object Value => value;

        public bool IsResolved => value != null;

        public FutureItem()
        {
        }

        public bool Resolve()
        {
            if (reference is FutureReference future)
                future.Resolve();

            value = reference.Target;
            return value != null;
        }

        public override string ToString()
        {
            return reference.ToString();
        }
    }

    public class FutureItem<T> : FutureItem, IFuture<T> where T : IFactoryItem
    {
        new public T Value => (T) value;

        object IFuture.Value => value;

        public FutureItem()
        {
        }

        public static implicit operator T(FutureItem<T> future)
        {
            return future.Value;
        }
    }

    public class FutureReference : IFuture, IFactoryItemReference
    {
        internal IFactoryItemReference reference;

        public object Value => reference;

        public bool IsResolved => reference == null || reference.Factory != null;

        public IFactory Factory => reference?.Factory;

        public IFactoryItem Target => reference?.Target;

        public Type ItemDefaultType => reference.ItemDefaultType;

        public FutureReference()
        {
        }

        public bool Resolve()
        {
            if (reference.Factory == null)
                return false;

            if (reference is IIndexedFactoryItemReference indexed)
                reference = indexed.Factory.GetReferenceTo(indexed.TargetIndex);
            else if (reference is INamedFactoryItemReference named)
                reference = named.Factory.GetReferenceTo(named.TargetName);
            else
                return false;

            return true;
        }

        public override string ToString()
        {
            return reference != null ? reference.ToString() : "reference not resolved yet";
        }

        public void Deserialize(ISerializer serializer)
        {
        }

        public void Serialize(ISerializer serializer)
        {
        }
    }

    public class FutureReference<ItemType, ReferenceType> : FutureReference, IFuture<ReferenceType>, IFactoryItemReference<ItemType>
        where ItemType : IFactoryItem
        where ReferenceType : IFactoryItemReference<ItemType>
    {
        new public ReferenceType Value => (ReferenceType) base.Value;

        object IFuture.Value => Value;

        IFactory<ItemType> IFactoryItemReference<ItemType>.Factory => Value?.Factory;

        ItemType IFactoryItemReference<ItemType>.Target => Value != null ? Value.Target : default;

        public FutureReference()
        {
        }

        public static implicit operator ReferenceType(FutureReference<ItemType, ReferenceType> future)
        {
            return future.Value;
        }

        public void Unset()
        {
            throw new NotImplementedException();
        }
    }

    public class FutureDelegate : IFuture
    {
        internal Type delegateType;
        internal MethodInfo method;
        internal IFuture target;

        private Delegate value;

        public Delegate Value
        {
            get
            {
                value ??= Delegate.CreateDelegate(delegateType, target.Value, method);
                return value;
            }
        }

        object IFuture.Value => Value;

        public bool IsResolved => target.IsResolved;

        public FutureDelegate(Type delegateType, MethodInfo method, IFuture target)
        {
            this.delegateType = delegateType;
            this.method = method;
            this.target = target;
        }

        public bool Resolve()
        {
            return target.Resolve();
        }

        public static implicit operator Delegate(FutureDelegate future)
        {
            return future.Value;
        }

        public override string ToString()
        {
            return $"{{delegate={delegateType} method={method} target={target}}}";
        }
    }

    public class FutureTuple : ITuple, IFuture
    {
        internal Type tupleType;
        internal object[] values;

        private ITuple value;

        public ITuple Value
        {
            get
            {
                value ??= TupleExtensions.ArrayToTuple(tupleType, values);
                return value;
            }
        }

        object IFuture.Value => Value;

        public object? this[int index] => values[index];

        public int Length => values.Length;

        public bool IsResolved
        {
            get
            {
                foreach (var value in values)
                {
                    if (value is not null and IFuture future)
                        if (!future.Resolve())
                            return false;
                }

                return true;
            }
        }

        public FutureTuple(Type tupleType, object[] values)
        {
            this.tupleType = tupleType;
            this.values = values;
        }

        public bool Resolve()
        {
            bool resolved = true;
            foreach (var value in values)
            {
                if (value is not null and IFuture future)
                    if (!future.Resolve())
                        resolved = false;
            }

            return resolved;
        }

        public override string ToString()
        {
            return $"{{values={values}}}";
        }
    }

    protected BinaryReader reader;
    protected BinaryWriter writer;

    private bool canDispose;

    private List<IFuture> futuresToResolve;
    private List<(Array array, int[] indices, IFuture future)> arrayElementsToResolve;
    private List<(IList list, int index, IFuture future)> listsToResolve;
    private List<(IDictionary dictionary, object key, IFuture future)> dictionariesToResolve;
    private List<(FieldInfo field, object instance, IFuture value)> fieldsToResolve;
    private List<(PropertyInfo property, object instance, IFuture value)> propertiesToResolve;
    private List<(EventInfo @event, object instance, FutureDelegate @delegate)> eventsToResolve;

    public IFuture Future
    {
        get;
        private set;
    } = null;

    public BinarySerializer(Stream stream)
    {
        reader = new BinaryReader(stream);
        writer = new BinaryWriter(stream);
        canDispose = true;

        CreateLists();
    }

    public BinarySerializer(BinaryReader reader)
    {
        this.reader = reader;
        canDispose = false;

        CreateLists();
    }

    public BinarySerializer(BinaryWriter writer)
    {
        this.writer = writer;
        canDispose = false;

        CreateLists();
    }

    private void CreateLists()
    {
        futuresToResolve = new List<IFuture>();
        arrayElementsToResolve = new List<(Array, int[], IFuture)>();
        listsToResolve = new List<(IList, int, IFuture)>();
        dictionariesToResolve = new List<(IDictionary, object, IFuture)>();
        fieldsToResolve = new List<(FieldInfo, object, IFuture)>();
        propertiesToResolve = new List<(PropertyInfo, object, IFuture)>();
        eventsToResolve = new List<(EventInfo, object, FutureDelegate)>();
    }

    public override byte ReadByte()
    {
        return reader.ReadByte();
    }

    public override sbyte ReadSByte()
    {
        return reader.ReadSByte();
    }

    public override short ReadShort()
    {
        return reader.ReadInt16();
    }

    public override ushort ReadUShort()
    {
        return reader.ReadUInt16();
    }

    public override int ReadInt()
    {
        return reader.ReadInt32();
    }

    public override uint ReadUInt()
    {
        return reader.ReadUInt32();
    }

    public override long ReadLong()
    {
        return reader.ReadInt64();
    }

    public override ulong ReadULong()
    {
        return reader.ReadUInt64();
    }

    public override float ReadFloat()
    {
        return reader.ReadSingle();
    }

    public override double ReadDouble()
    {
        return reader.ReadDouble();
    }

    public override bool ReadBool()
    {
        return reader.ReadBoolean();
    }

    public override char ReadChar()
    {
        return reader.ReadChar();
    }

    public override string ReadString(bool nullable = true)
    {
        if (nullable)
        {
            bool isSet = ReadBool();
            if (!isSet)
                return null;
        }

        return reader.ReadString();
    }

    public override FixedSingle ReadFixedSingle()
    {
        int rawValue = ReadInt();
        return FixedSingle.FromRawValue(rawValue);
    }

    public override FixedDouble ReadFixedDouble()
    {
        long rawValue = ReadLong();
        return FixedDouble.FromRawValue(rawValue);
    }

    public override Interval ReadInterval()
    {
        byte flag = ReadByte();
        FixedSingle min = ReadFixedSingle();
        FixedSingle max = ReadFixedSingle();
        return Interval.MakeInterval((min, (flag & 1) != 0), (max, (flag & 2) != 0));
    }

    public override Vector ReadVector()
    {
        FixedSingle x = ReadFixedSingle();
        FixedSingle y = ReadFixedSingle();
        return new Vector(x, y);
    }

    public override LineSegment ReadLineSegment()
    {
        Vector start = ReadVector();
        Vector end = ReadVector();
        return new LineSegment(start, end);
    }

    public override Box ReadBox()
    {
        Vector origin = ReadVector();
        Vector mins = ReadVector();
        Vector maxs = ReadVector();
        return new Box(origin, mins, maxs);
    }

    public override RightTriangle ReadRightTriangle()
    {
        Vector origin = ReadVector();
        FixedSingle hCathetus = ReadFixedSingle();
        FixedSingle vCathetus = ReadFixedSingle();
        return new RightTriangle(origin, hCathetus, vCathetus);
    }

    public override T ReadEnum<T>()
    {
        return (T) ReadValue(Enum.GetUnderlyingType(typeof(T)));
    }

    public override void WriteByte(byte value)
    {
        writer.Write(value);
    }

    public override void WriteSByte(sbyte value)
    {
        writer.Write(value);
    }

    public override void WriteShort(short value)
    {
        writer.Write(value);
    }

    public override void WriteUShort(ushort value)
    {
        writer.Write(value);
    }

    public override void WriteInt(int value)
    {
        writer.Write(value);
    }

    public override void WriteUInt(uint value)
    {
        writer.Write(value);
    }

    public override void WriteLong(long value)
    {
        writer.Write(value);
    }

    public override void WriteULong(ulong value)
    {
        writer.Write(value);
    }

    public override void WriteFloat(float value)
    {
        writer.Write(value);
    }

    public override void WriteDouble(double value)
    {
        writer.Write(value);
    }

    public override void WriteBool(bool value)
    {
        writer.Write(value);
    }

    public override void WriteChar(char value)
    {
        writer.Write(value);
    }

    public override void WriteString(string value, bool nullable = true)
    {
        if (nullable)
        {
            if (value == null)
            {
                WriteBool(false);
                return;
            }

            WriteBool(true);
        }

        writer.Write(value);
    }

    public override void WriteFixedSingle(FixedSingle value)
    {
        WriteInt(value.RawValue);
    }

    public override void WriteFixedDouble(FixedDouble value)
    {
        WriteLong(value.RawValue);
    }

    public override void WriteInterval(Interval value)
    {
        byte flags = (byte) ((value.IsClosedLeft ? 1 : 0) | (value.IsClosedRight ? 2 : 0));
        WriteByte(flags);
        WriteFixedSingle(value.Min);
        WriteFixedSingle(value.Max);
    }

    public override void WriteVector(Vector value)
    {
        WriteFixedSingle(value.X);
        WriteFixedSingle(value.Y);
    }

    public override void WriteLineSegment(LineSegment value)
    {
        WriteVector(value.Start);
        WriteVector(value.End);
    }

    public override void WriteBox(Box value)
    {
        WriteVector(value.Origin);
        WriteVector(value.Mins);
        WriteVector(value.Maxs);
    }

    public override void WriteRightTriangle(RightTriangle value)
    {
        WriteVector(value.Origin);
        WriteFixedSingle(value.RawHCathetus);
        WriteFixedSingle(value.RawVCathetus);
    }

    public override void WriteEnum<T>(T value)
    {
        WriteValue(Enum.GetUnderlyingType(typeof(T)), value);
    }

    public Type ReadType()
    {
        string name = ReadString(false);
        return Type.GetType(name, true);
    }

    public void WriteType(Type type)
    {
        WriteString(type.AssemblyQualifiedName, false);
    }

    public Type[] ReadTypes()
    {
        int count = ReadInt();
        var result = new Type[count];
        for (int i = 0; i < count; i++)
            result[i] = ReadType();

        return result;
    }

    public void WriteTypes(IEnumerable<Type> types)
    {
        WriteInt(types.Count());
        foreach (var type in types)
            WriteType(type);
    }

    private int[] GetIndices(int[] lengths, int absoluteIndex)
    {
        int[] result = new int[lengths.Length];

        for (int i = result.Length - 1; i > 0; i--)
        {
            result[i] = absoluteIndex % lengths[i];
            absoluteIndex /= lengths[i];
        }

        result[0] = absoluteIndex;
        return result;
    }

    public Array ReadArray(Type arrayType, bool nullable = true)
    {
        if (nullable)
        {
            bool isSet = ReadBool();
            if (!isSet)
                return null;
        }

        var elementType = arrayType.GetElementType();

        int count = ReadInt();
        int rank = arrayType.GetArrayRank();

        Array result;

        if (rank > 1)
        {
            int[] lengths = new int[rank];
            lengths[0] = count;

            for (int i = 1; i < rank; i++)
            {
                int length = ReadInt();
                lengths[i] = length;
                count *= length;
            }

            result = Array.CreateInstance(elementType, lengths);

            for (int i = 0; i < count; i++)
            {
                var indices = GetIndices(lengths, i);
                var value = ReadValue(elementType);

                if (value == null && Future != null)
                    arrayElementsToResolve.Add((result, indices, Future));
                else
                    result.SetValue(value, indices);
            }
        }
        else
        {
            result = Array.CreateInstance(elementType, count);

            for (int i = 0; i < count; i++)
            {
                var value = ReadValue(elementType);

                if (value == null && Future != null)
                    arrayElementsToResolve.Add((result, new int[] { i }, Future));
                else
                    result.SetValue(value, i);
            }
        }

        return result;
    }

    public void WriteArray(Type arrayType, Array array, bool nullable = true)
    {
        if (nullable)
        {
            if (array == null)
            {
                WriteBool(false);
                return;
            }

            WriteBool(true);
        }

        var elementType = arrayType.GetElementType();

        int rank = arrayType.GetArrayRank();
        if (rank > 1)
        {
            for (int i = 0; i < rank; i++)
                WriteInt(array.GetLength(i));
        }
        else
        {
            WriteInt(array.Length);
        }

        foreach (var element in array)
            WriteValue(elementType, element);
    }

    public IList ReadList(bool nullable = true)
    {
        if (nullable)
        {
            bool isSet = ReadBool();
            if (!isSet)
                return null;
        }

        var listType = ReadType();
        Type elementType = listType.IsGenericType && listType.GetGenericTypeDefinition().IsAssignableTo(typeof(List<>)) ? listType.GetGenericArguments()[0] : null;

        var result = (IList) Activator.CreateInstance(listType);
        ReadList(result, elementType);
        return result;
    }

    public void ReadList(IList list, Type elementType)
    {
        int count = ReadInt();

        for (int i = 0; i < count; i++)
        {
            var element = elementType != null ? ReadValue(elementType) : ReadValue();

            if (element == null && Future != null)
            {
                listsToResolve.Add((list, i, Future));
                list.Add(null);
            }
            else
                list.Add(element);
        }
    }

    public IList<Element> ReadList<Element>(bool nullable = true)
    {
        return (IList<Element>) ReadList(nullable);
    }

    public void ReadList<Element>(IList<Element> list)
    {
        ReadList((IList) list, typeof(Element));
    }

    public void WriteList(IList list, bool nullable = true)
    {
        if (nullable)
        {
            if (list == null)
            {
                WriteBool(false);
                return;
            }

            WriteBool(true);
        }

        var listType = list.GetType();
        Type elementType = listType.IsGenericType && listType.GetGenericTypeDefinition().IsAssignableTo(typeof(List<>)) ? listType.GetGenericArguments()[0] : null;

        WriteType(listType);
        WriteList(list, elementType);
    }

    public void WriteList(IList list, Type elementType)
    {
        WriteInt(list.Count);

        foreach (var element in list)
        {
            if (elementType != null)
                WriteValue(elementType, element);
            else
                WriteValue(element, true, false, false, true);
        }
    }

    public void WriteList<Element>(IList<Element> list, bool nullable)
    {
        WriteList((IList) list, nullable);
    }

    public void WriteList<Element>(IList<Element> list)
    {
        WriteList((IList) list, typeof(Element));
    }

    public IDictionary ReadDictionary(bool nullable = true)
    {
        if (nullable)
        {
            bool isSet = ReadBool();
            if (!isSet)
                return null;
        }

        var dictionaryType = ReadType();

        Type keyType = null;
        Type valueType = null;

        if (dictionaryType.IsGenericType && dictionaryType.GetGenericTypeDefinition().IsAssignableTo(typeof(Dictionary<,>)))
        {
            keyType = dictionaryType.GetGenericArguments()[0];
            valueType = dictionaryType.GetGenericArguments()[1];
        }

        var result = (IDictionary) Activator.CreateInstance(dictionaryType);
        ReadDictionary(result, keyType, valueType);
        return result;
    }

    public IDictionary ReadDictionary(IDictionary dictionary, Type keyType, Type valueType)
    {
        int count = ReadInt();

        for (int i = 0; i < count; i++)
        {
            var key = keyType != null ? ReadValue(keyType) : ReadValue();
            var value = valueType != null ? ReadValue(valueType) : ReadValue();

            if (value == null && Future != null)
            {
                dictionariesToResolve.Add((dictionary, key, Future));
                dictionary.Add(key, null);
            }
            else
                dictionary.Add(key, value);
        }

        return dictionary;
    }

    public IDictionary<Key, Value> ReadDictionary<Key, Value>(bool nullable = true)
    {
        return (IDictionary<Key, Value>) ReadDictionary(nullable);
    }

    public void ReadDictionary<Key, Value>(IDictionary<Key, Value> dictionary)
    {
        ReadDictionary((IDictionary) dictionary, typeof(Key), typeof(Value));
    }

    public void WriteDictionary(IDictionary dictionary, bool nullable = true)
    {
        if (nullable)
        {
            if (dictionary == null)
            {
                WriteBool(false);
                return;
            }

            WriteBool(true);
        }

        var dictionaryType = dictionary.GetType();

        Type keyType = null;
        Type valueType = null;

        if (dictionaryType.IsGenericType && dictionaryType.GetGenericTypeDefinition().IsAssignableTo(typeof(Dictionary<,>)))
        {
            keyType = dictionaryType.GetGenericArguments()[0];
            valueType = dictionaryType.GetGenericArguments()[1];
        }

        WriteType(dictionaryType);
        WriteDictionary(dictionary, keyType, valueType);
    }

    public void WriteDictionary(IDictionary dictionary, Type keyType, Type valueType)
    {
        WriteInt(dictionary.Count);

        foreach (var key in dictionary.Keys)
        {
            var value = dictionary[key];

            if (keyType != null)
                WriteValue(keyType, key);
            else
                WriteValue(key, true, false, false, true);

            if (valueType != null)
                WriteValue(valueType, value);
            else
                WriteValue(value, true, false, false, true);
        }
    }

    public void WriteDictionary<KeyType, ValueType>(IDictionary<KeyType, ValueType> dictionary, bool nullable)
    {
        WriteDictionary((IDictionary) dictionary, nullable);
    }

    public void WriteDictionary<KeyType, ValueType>(IDictionary<KeyType, ValueType> dictionary)
    {
        WriteDictionary((IDictionary) dictionary, typeof(KeyType), typeof(ValueType));
    }

    public MethodInfo ReadMethodInfo()
    {
        var declaringType = ReadType();
        string name = ReadString(false);
        var types = ReadTypes();
        bool isStatic = ReadBool();
        return declaringType.GetMethod(name, (isStatic ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly, types);
    }

    public void WriteMethodInfo(MethodInfo info)
    {
        WriteType(info.DeclaringType);
        WriteString(info.Name, false);
        WriteTypes(info.GetParameters().Select((p) => p.ParameterType));
        WriteBool(info.IsStatic);
    }

    public override object ReadDelegate(bool nullable = true)
    {
        Future = null;

        if (nullable)
        {
            bool isSet = ReadBool();
            if (!isSet)
                return null;
        }

        var delegateType = ReadType();
        object target = ReadValue(false, false, true);
        var methodInfo = ReadMethodInfo();

        if (target == null && Future != null)
        {
            Future = new FutureDelegate(delegateType, methodInfo, Future);
            return null;
        }

        return Delegate.CreateDelegate(delegateType, target, methodInfo);
    }

    public override void WriteDelegate(Delegate @delegate, bool nullable = true)
    {
        if (nullable)
        {
            if (@delegate == null)
            {
                WriteBool(false);
                return;
            }

            WriteBool(true);
        }

        WriteType(@delegate.GetType());

        var target = @delegate.Target;
        WriteValue(target, true, false, false, true);

        WriteMethodInfo(@delegate.Method);
    }

    public object ReadValueTuple(Type tupleType)
    {
        Future = null;

        bool hasFutures = false;
        var types = tupleType.GetGenericArguments();
        var values = new object[types.Length];

        for (int i = 0; i < types.Length; i++)
        {
            var type = types[i];
            var value = ReadValue(type);
            values[i] = value;

            if (value is IFuture)
                hasFutures = true;
        }

        if (hasFutures)
        {
            Future = new FutureTuple(tupleType, values);
            return null;
        }

        return TupleExtensions.ArrayToTuple(tupleType, values);
    }

    public void WriteValueTuple(Type tupleType, ITuple tuple)
    {
        var types = tupleType.GetGenericArguments();
        for (int i = 0; i < types.Length; i++)
        {
            var type = types[i];
            var value = tuple[i];
            WriteValue(type, value);
        }
    }

    public override IFactoryItemReference ReadItemReference(Type referenceType, bool nullable = true)
    {
        Future = null;

        if (nullable)
        {
            bool isSet = ReadBool();
            if (!isSet)
                return null;
        }

        var fakeReference = (IFactoryItemReference) FormatterServices.GetUninitializedObject(referenceType);
        fakeReference.Deserialize(this);
        var factory = fakeReference.Factory;
        if (factory == null)
        {
            var futureType = typeof(FutureReference<,>).MakeGenericType(fakeReference.ItemDefaultType, referenceType);
            var future = (FutureReference) Activator.CreateInstance(futureType);
            future.reference = fakeReference;
            futuresToResolve.Add(future);
            Future = future;
            return null;
        }

        if (fakeReference is IIndexedFactoryItemReference indexed)
            return indexed.Factory.GetReferenceTo(indexed.TargetIndex);

        if (fakeReference is INamedFactoryItemReference named)
            return named.Factory.GetReferenceTo(named.TargetName);

        return fakeReference;
    }

    public override void WriteItemReference(IFactoryItemReference reference, bool nullable = true)
    {
        if (nullable)
        {
            if (reference == null)
            {
                WriteBool(false);
                return;
            }

            WriteBool(true);
        }

        reference.Serialize(this);
    }

    public void DeserializeEvent(object instance, EventInfo eventInfo, bool nullable = true)
    {
        if (nullable)
        {
            bool isSet = ReadBool();
            if (!isSet)
                return;
        }

        for (var type = instance.GetType(); type != null; type = type.BaseType)
        {
            var field = type.GetField(eventInfo.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (field != null)
            {
                var @event = (MulticastDelegate) field.GetValue(instance);
                if (@event != null)
                {
                    foreach (var handler in @event.GetInvocationList())
                        eventInfo.RemoveEventHandler(instance, handler);
                }

                break;
            }
        }

        int count = ReadInt();
        for (int i = 0; i < count; i++)
        {
            var handler = ReadDelegate();

            if (handler == null)
            {
                if (Future == null)
                    continue;

                eventsToResolve.Add((eventInfo, instance, (FutureDelegate) Future));
            }
            else
                eventInfo.AddEventHandler(instance, (Delegate) handler);
        }
    }

    public void SerializeEvent(MulticastDelegate @event, bool nullable = true)
    {
        if (nullable)
        {
            if (@event == null)
            {
                WriteBool(false);
                return;
            }

            WriteBool(true);
        }

        var invocationList = @event.GetInvocationList();
        WriteInt(invocationList.Length);
        foreach (var handler in invocationList)
            WriteDelegate(handler);
    }

    protected bool OnReadObject(Type type, ref object obj)
    {
        return false;
    }

    protected override void OnFieldDeserialized(FieldInfo field, object instance, string name, object value)
    {
        if (value == null && Future != null)
            fieldsToResolve.Add((field, instance, Future));
        else
            field.SetValue(instance, value);
    }

    protected override void OnPropertyDeserialized(PropertyInfo property, object instance, string name, object value)
    {
        if (value == null && Future != null)
            propertiesToResolve.Add((property, instance, Future));
        else
            property.SetValue(instance, value);
    }

    private bool IsBasicType(Type type)
    {
        return type == typeof(bool)
            || type == typeof(byte)
            || type == typeof(sbyte)
            || type == typeof(short)
            || type == typeof(ushort)
            || type == typeof(int)
            || type == typeof(uint)
            || type == typeof(long)
            || type == typeof(ulong)
            || type == typeof(float)
            || type == typeof(double)
            || type == typeof(char)
            || type == typeof(string)
            || type == typeof(FixedSingle)
            || type == typeof(FixedDouble)
            || type == typeof(Interval)
            || type == typeof(Vector)
            || type == typeof(LineSegment)
            || type == typeof(Box)
            || type == typeof(RightTriangle)
            || type.IsAssignableTo(typeof(Delegate))
            || type.IsEnum;
    }

    public override object ReadObject(bool acceptNonSerializable = false, bool ignoreItems = false, bool nullable = true)
    {
        Future = null;

        if (nullable)
        {
            bool isSet = ReadBool();
            if (!isSet)
                return null;
        }

        var objectType = ReadType();

        if (IsBasicType(objectType))
            return ReadValue(objectType, false, false, false);

        if (!ignoreItems && objectType.IsAssignableTo(typeof(IFactoryItem)))
        {
            bool isSet = ReadBool();
            if (!isSet)
                return null;

            var referenceType = ReadType();
            var tempReference = ReadItemReference(referenceType, false);
            if (tempReference == null && Future != null)
                tempReference = (FutureReference) Future;

            var futureType = typeof(FutureItem<>).MakeGenericType(objectType);
            var future = (FutureItem) Activator.CreateInstance(futureType);

            future.reference = tempReference;
            futuresToResolve.Add(future);
            Future = future;

            return null;
        }

        object obj = null;
        if (!OnReadObject(objectType, ref obj))
        {
            if (objectType.IsAssignableTo(typeof(ISerializable)))
            {
                var serializable = (ISerializable) FormatterServices.GetUninitializedObject(objectType);
                serializable.Deserialize(this);
                obj = serializable;
            }
            else
            {
                if (!acceptNonSerializable && !objectType.Name.StartsWith("<>c__DisplayClass") && (SerializableAttribute) Attribute.GetCustomAttribute(objectType, typeof(SerializableAttribute)) == null)
                    throw new Exception($"Type '{objectType}' is not serializable. Use the attribute 'NotSerialize' if you dont want to serialize members of this type.");

                obj = FormatterServices.GetUninitializedObject(objectType);

                for (var type = objectType; type != null; type = type.BaseType)
                {
                    var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    foreach (var field in fields)
                    {
                        var attribute = Attribute.GetCustomAttribute(field, typeof(NotSerializableAttribute));
                        if (attribute != null)
                            continue;

                        string name = field.Name;
                        var fieldType = field.FieldType;
                        if (!fieldType.IsAssignableTo(typeof(MulticastDelegate)))
                            DeserializeField(field, obj);
                    }

                    var events = type.GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    foreach (var eventInfo in events)
                    {
                        var field = type.GetField(eventInfo.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                        var attribute = Attribute.GetCustomAttribute(eventInfo, typeof(NotSerializableAttribute));
                        if (attribute != null)
                            continue;

                        DeserializeEvent(obj, eventInfo);
                    }
                }
            }
        }

        if (obj is ISerializableListener listener)
            listener.OnDeserialized(this);

        return obj;
    }

    protected virtual bool OnWriteObject(object obj)
    {
        return false;
    }

    public override void WriteObject(object obj, bool acceptNonSerializable = false, bool ignoreItems = false, bool nullable = true)
    {
        if (nullable)
        {
            if (obj == null)
            {
                WriteBool(false);
                return;
            }

            WriteBool(true);
        }

        var objectType = obj.GetType();
        WriteType(objectType);

        if (IsBasicType(objectType))
        {
            WriteValue(objectType, obj, false, false, false);
            return;
        }

        if (!ignoreItems && obj is IFactoryItem item)
        {
            var factory = item.Factory;
            var reference = factory.GetReferenceTo(item);

            if (reference != null)
            {
                var referenceType = reference.GetType();

                WriteBool(true);
                WriteType(referenceType);
                WriteItemReference(reference, false);
            }
            else
            {
                WriteBool(false);
            }

            return;
        }

        if (!OnWriteObject(obj))
        {
            if (obj is ISerializable serializable)
            {
                serializable.Serialize(this);
            }
            else
            {
                if (!acceptNonSerializable && !objectType.Name.StartsWith("<>c__DisplayClass") && Attribute.GetCustomAttribute(objectType, typeof(SerializableAttribute)) == null)
                    throw new Exception($"Type '{objectType}' is not serializable. Use the attribute 'NotSerialize' if you dont want to serialize members of this type.");

                for (var type = objectType; type != null; type = type.BaseType)
                {
                    var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    foreach (var field in fields)
                    {
                        var attribute = Attribute.GetCustomAttribute(field, typeof(NotSerializableAttribute));
                        if (attribute != null)
                            continue;

                        string name = field.Name;
                        var fieldType = field.FieldType;
                        if (!fieldType.IsAssignableTo(typeof(MulticastDelegate)))
                        {
                            object value = field.GetValue(obj);
                            WriteValue(fieldType, value);
                        }
                    }

                    var events = type.GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    foreach (var eventInfo in events)
                    {
                        var field = type.GetField(eventInfo.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                        var attribute = Attribute.GetCustomAttribute(eventInfo, typeof(NotSerializableAttribute));
                        if (attribute != null)
                            continue;

                        var value = (MulticastDelegate) field.GetValue(obj);
                        SerializeEvent(value);
                    }
                }
            }
        }

        if (obj is ISerializableListener listener)
            listener.OnSerialized(this);
    }

    public virtual object ReadValue(Type type, bool acceptNonSerializable = false, bool ignoreItems = false, bool nullable = true)
    {
        Future = null;

        if (type == typeof(bool))
            return ReadBool();

        if (type == typeof(byte))
            return ReadByte();

        if (type == typeof(sbyte))
            return ReadSByte();

        if (type == typeof(short))
            return ReadShort();

        if (type == typeof(ushort))
            return ReadUShort();

        if (type == typeof(int))
            return ReadInt();

        if (type == typeof(uint))
            return ReadUInt();

        if (type == typeof(long))
            return ReadLong();

        if (type == typeof(ulong))
            return ReadULong();

        if (type == typeof(float))
            return ReadFloat();

        if (type == typeof(double))
            return ReadDouble();

        if (type == typeof(char))
            return ReadChar();

        if (type == typeof(string))
            return ReadString(nullable);

        if (type == typeof(FixedSingle))
            return ReadFixedSingle();

        if (type == typeof(FixedDouble))
            return ReadFixedDouble();

        if (type == typeof(Interval))
            return ReadInterval();

        if (type == typeof(Vector))
            return ReadVector();

        if (type == typeof(LineSegment))
            return ReadLineSegment();

        if (type == typeof(Box))
            return ReadBox();

        if (type == typeof(RightTriangle))
            return ReadRightTriangle();

        if (type.IsAssignableTo(typeof(Delegate)))
            return ReadDelegate(nullable);

        if (type.IsAssignableTo(typeof(IFactoryItemReference)))
            return ReadItemReference(type, nullable);

        if (type.IsEnum)
            return Enum.ToObject(type, ReadValue(Enum.GetUnderlyingType(type)));

        if (type.IsArray)
            return ReadArray(type, nullable);

        if (type.IsAssignableTo(typeof(IList)))
            return ReadList(nullable);

        if (type.IsAssignableTo(typeof(IDictionary)))
            return ReadDictionary(nullable);

        if (type.IsGenericType)
        {
            var generic = type.GetGenericTypeDefinition();
            if (generic == typeof(ValueTuple<,>)
                || generic == typeof(ValueTuple<,,>)
                || generic == typeof(ValueTuple<,,,>)
                || generic == typeof(ValueTuple<,,,,>)
                || generic == typeof(ValueTuple<,,,,,>)
                || generic == typeof(ValueTuple<,,,,,,>)
                || generic == typeof(ValueTuple<,,,,,,,>))
                return ReadValueTuple(type);
        }

        var obj = ReadObject(acceptNonSerializable, ignoreItems, nullable);
        if (obj == null)
            return null;

        var objType = obj.GetType();
        if (!objType.IsAssignableTo(type))
            throw new InvalidCastException($"Can't cast '{objType.Name}' to '{type.Name}'.");

        return obj;
    }

    public virtual void WriteValue(Type type, object value, bool acceptNonSerializable = false, bool ignoreItems = false, bool nullable = true)
    {
        if (type == typeof(bool))
            WriteBool((bool) value);
        else if (type == typeof(byte))
            WriteByte((byte) value);
        else if (type == typeof(sbyte))
            WriteSByte((sbyte) value);
        else if (type == typeof(short))
            WriteShort((short) value);
        else if (type == typeof(ushort))
            WriteUShort((ushort) value);
        else if (type == typeof(int))
            WriteInt((int) value);
        else if (type == typeof(uint))
            WriteUInt((uint) value);
        else if (type == typeof(long))
            WriteLong((long) value);
        else if (type == typeof(ulong))
            WriteULong((ulong) value);
        else if (type == typeof(float))
            WriteFloat((float) value);
        else if (type == typeof(double))
            WriteDouble((double) value);
        else if (type == typeof(char))
            WriteChar((char) value);
        else if (type == typeof(string))
            WriteString((string) value, nullable);
        else if (type == typeof(FixedSingle))
            WriteFixedSingle((FixedSingle) value);
        else if (type == typeof(FixedDouble))
            WriteFixedDouble((FixedDouble) value);
        else if (type == typeof(Interval))
            WriteInterval((Interval) value);
        else if (type == typeof(Vector))
            WriteVector((Vector) value);
        else if (type == typeof(LineSegment))
            WriteLineSegment((LineSegment) value);
        else if (type == typeof(Box))
            WriteBox((Box) value);
        else if (type == typeof(RightTriangle))
            WriteRightTriangle((RightTriangle) value);
        else if (type.IsAssignableTo(typeof(Delegate)))
            WriteDelegate((Delegate) value, nullable);
        else if (type.IsAssignableTo(typeof(IFactoryItemReference)))
            WriteItemReference((IFactoryItemReference) value, nullable);
        else if (type.IsEnum)
            WriteValue(Enum.GetUnderlyingType(type), value);
        else if (type.IsArray)
            WriteArray(type, (Array) value, nullable);
        else if (type.IsAssignableTo(typeof(IList)))
            WriteList((IList) value, nullable);
        else if (type.IsAssignableTo(typeof(IDictionary)))
            WriteDictionary((IDictionary) value, nullable);
        else if (type.IsGenericType)
        {
            var generic = type.GetGenericTypeDefinition();
            if (generic == typeof(ValueTuple<,>)
                || generic == typeof(ValueTuple<,,>)
                || generic == typeof(ValueTuple<,,,>)
                || generic == typeof(ValueTuple<,,,,>)
                || generic == typeof(ValueTuple<,,,,,>)
                || generic == typeof(ValueTuple<,,,,,,>)
                || generic == typeof(ValueTuple<,,,,,,,>))
            {
                WriteValueTuple(type, (ITuple) value);
                return;
            }

            WriteObject(value, acceptNonSerializable, ignoreItems, nullable);
        }
        else
            WriteObject(value, acceptNonSerializable, ignoreItems, nullable);
    }

    public object ReadValue(bool acceptNonSerializable = false, bool ignoreItems = false, bool nullable = true)
    {
        if (nullable)
        {
            bool isSet = ReadBool();
            if (!isSet)
                return null;
        }

        var type = ReadType();
        return ReadValue(type, acceptNonSerializable, ignoreItems);
    }

    public T ReadValue<T>(bool acceptNonSerializable = false, bool ignoreItems = false, bool nullable = true)
    {
        return (T) ReadValue(typeof(T), acceptNonSerializable, ignoreItems, nullable);
    }

    public void WriteValue(object value, bool writeType = false, bool acceptNonSerializable = false, bool ignoreItems = false, bool nullable = true)
    {
        if (nullable)
        {
            if (value == null)
            {
                WriteBool(false);
                return;
            }

            WriteBool(true);
        }

        var type = value.GetType();
        if (writeType)
            WriteType(type);

        WriteValue(type, value, acceptNonSerializable);
    }

    public void WriteValue<T>(T value, bool acceptNonSerializable = false, bool ignoreItems = false, bool nullable = true)
    {
        WriteValue(typeof(T), value, acceptNonSerializable, ignoreItems, nullable);
    }

    public override object DeserializeObject(string name)
    {
        return ReadValue();
    }

    public override object DeserializeObject(Type type, string name)
    {
        return ReadValue(type);
    }

    public override void SerializeObject(string name, object obj)
    {
        WriteValue(obj, true, false, false);
    }

    public void Resolve()
    {
        foreach (var future in futuresToResolve)
            future.Resolve();

        foreach (var future in futuresToResolve)
            if (!future.IsResolved)
                throw new Exception($"Can't resolve future {future}.");

        foreach (var (array, indices, future) in arrayElementsToResolve)
            array.SetValue(future.Value, indices);

        foreach (var (list, index, future) in listsToResolve)
            list[index] = future.Value;

        foreach (var (dictionary, key, future) in dictionariesToResolve)
            dictionary[key] = future.Value;

        foreach (var (field, instance, future) in fieldsToResolve)
            field.SetValue(instance, future.Value);

        foreach (var (property, instance, future) in propertiesToResolve)
            property.SetValue(instance, future.Value);

        foreach (var (@event, instance, @delegate) in eventsToResolve)
            @event.AddEventHandler(instance, @delegate.Value);
    }

    public void Dispose()
    {
        if (canDispose)
        {
            futuresToResolve.Clear();
            arrayElementsToResolve.Clear();
            dictionariesToResolve.Clear();
            fieldsToResolve.Clear();
            propertiesToResolve.Clear();
            eventsToResolve.Clear();

            reader.Dispose();
            writer.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}