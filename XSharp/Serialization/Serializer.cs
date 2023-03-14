using System;
using System.Reflection;

namespace XSharp.Serialization;

public abstract class Serializer
{
    public abstract void SerializeObject(string name, object obj);

    public abstract object DeserializeObject(string name);

    public abstract object DeserializeObject(Type type, string name);

    public void SerializeField(string name, object instance)
    {
        Type type = instance.GetType();
        var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        var value = field.GetValue(instance);
        SerializeObject(name, value);
    }

    public void DeserializeField(string name, object instance)
    {
        Type type = instance.GetType();
        var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        DeserializeField(field, instance);
    }

    public void DeserializeField(FieldInfo field, object instance)
    {
        string name = field.Name;
        var fieldType = field.FieldType;
        var value = DeserializeObject(fieldType, name);
        OnFieldDeserialized(field, instance, name, value);
    }

    public void DeserializeProperty(string name, object instance)
    {
        Type type = instance.GetType();
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
}