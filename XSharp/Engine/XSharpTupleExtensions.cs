using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace XSharp.Engine
{
    public static class XSharpTupleExtensions
    {
        public static bool CanConvertTupleToArray<ArrayElementType>(Type tupleType)
        {
            Type[] typeParameters = tupleType.GetGenericArguments();
            Type elementType = typeof(ArrayElementType);
            TypeConverter conv = TypeDescriptor.GetConverter(elementType);

            for (int i = 0; i < typeParameters.Length; i++)
            {
                var typeParam = typeParameters[i];
                if (typeParam != elementType && !elementType.IsAssignableFrom(typeParam) && !conv.CanConvertFrom(typeParam))
                    return false;
            }

            return true;
        }

        public static bool CanConvertTupleToArray<ArrayElementType, TupleType>() where TupleType : ITuple
        {
            Type tupleType = typeof(TupleType);
            return CanConvertTupleToArray<ArrayElementType>(tupleType);
        }

        public static ArrayElementType[] ToArray<ArrayElementType>(this ITuple tuple)
        {
            Type tupleType = tuple.GetType();
            Type[] typeParameters = tupleType.GetGenericArguments();
            Type elementType = typeof(ArrayElementType);
            TypeConverter conv = TypeDescriptor.GetConverter(elementType);
            var args = new ArrayElementType[typeParameters.Length];

            for (int i = 0; i < typeParameters.Length; i++)
            {
                var typeParam = typeParameters[i];
                var param = tuple[i];
                if (typeParam != elementType && !typeParam.IsAssignableFrom(elementType))
                    param = conv.ConvertFrom(param);

                args[i] = (ArrayElementType) param;
            }

            return args;
        }

        public static bool CanConvertArrayToTuple<ArrayElementType, TupleType>() where TupleType : ITuple
        {
            Type tupleType = typeof(TupleType);
            Type[] typeParameters = tupleType.GetGenericArguments();
            Type elementType = typeof(ArrayElementType);

            for (int i = 0; i < typeParameters.Length; i++)
            {
                var typeParam = typeParameters[i];
                TypeConverter conv = TypeDescriptor.GetConverter(typeParam);
                if (typeParam != elementType && !typeParam.IsAssignableFrom(elementType) && !conv.CanConvertFrom(elementType))
                    return false;
            }

            return true;
        }

        public static ITuple ArrayToTuple<ArrayElementType>(Type tupleType, params ArrayElementType[] array)
        {
            Type[] typeParameters = tupleType.GetGenericArguments();
            Type elementType = typeof(ArrayElementType);
            var args = new object[typeParameters.Length];

            for (int i = 0; i < typeParameters.Length; i++)
            {
                var typeParam = typeParameters[i];
                ArrayElementType arg = array[i];
                if (typeParam != elementType && !elementType.IsAssignableFrom(typeParam))
                {
                    TypeConverter conv = TypeDescriptor.GetConverter(typeParam);
                    arg = (ArrayElementType) conv.ConvertFrom(arg);
                }

                args[i] = arg;
            }

            return (ITuple) Activator.CreateInstance(tupleType, args);
        }

        public static TupleType ArrayToTuple<ArrayElementType, TupleType>(params ArrayElementType[] array) where TupleType : ITuple
        {
            Type tupleType = typeof(TupleType);
            return (TupleType) ArrayToTuple(tupleType, array);
        }
    }
}