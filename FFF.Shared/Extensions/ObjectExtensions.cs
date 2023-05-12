using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace FFF.Shared
{
    public static class ObjectExtensions
    {
        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        public static bool AreEqual(this object object1, object object2)
        {
            bool flag1 = object1 == null;
            bool flag2 = object2 == null;
            if (flag1 & flag2)
                return true;
            if (flag1 | flag2)
                return false;
            if (object1 == object2)
                return true;
            string strA = object1 as string;
            string strB = object2 as string;
            return strA != null && strB != null ? string.Compare(strA, strB, StringComparison.Ordinal) == 0 : object1.Equals(object2);
        }

        public static R TryGet<T, R>(this T target, Func<T, R> getter) where T : class where R : class =>
            (object)target != null ? getter(target) : default(R);

        public static T DeepClone<T>(this T input) where T : ISerializable
        {
            using (MemoryStream stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, input);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }

        public static T GetPropertyValue<T>(this object source, string property)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var sourceType = source.GetType();
            var sourceProperties = sourceType.GetProperties();

            var propertyValue = (from s in sourceProperties
                                 where s.Name.Equals(property)
                                 select s.GetValue(source, null)).FirstOrDefault();

            return propertyValue != null ? (T)propertyValue : default(T);
        }

        public static U ChangeType<U>(this object source, U returnValueIfException)
        {
            try
            {
                return source.ChangeType<U>();
            }
            catch
            {
                return returnValueIfException;
            }
        }

        public static U ChangeType<U>(this object source)
        {
            if (source is U)
                return (U)source;

            var destinationType = typeof(U);
            if (destinationType.IsGenericType && destinationType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                destinationType = new NullableConverter(destinationType).UnderlyingType;

            return (U)Convert.ChangeType(source, destinationType);
        }

        public static string ToStringDump(this object instance)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            var propInfos = instance.GetType().GetProperties(flags);

            var sb = new StringBuilder();

            string typeName = instance.GetType().Name;
            sb.AppendLine(typeName);
            sb.AppendLine("".PadRight(typeName.Length + 5, '='));

            foreach (System.Reflection.PropertyInfo propInfo in propInfos)
            {
                sb.Append(propInfo.Name);
                sb.Append(": ");
                if (propInfo.GetIndexParameters().Length > 0)
                {
                    sb.Append("Indexed Property cannot be used");
                }
                else
                {
                    object value = propInfo.GetValue(instance, null);
                    sb.Append(value != null ? value : "null");
                }
                sb.Append(System.Environment.NewLine);
            }

            return sb.ToString();
        }

        public static T As<T>(this object obj) where T : class =>
             obj as T;

        public static T CastTo<T>(this object obj) =>
             (T)obj;

        public static object Call(this object obj, string methodName, params object[] parameters)
        {
            var type = obj.GetType();
            var method = type.GetMethod(methodName, Flags, null, parameters == null ? Type.EmptyTypes : parameters.Select(p => p.GetType()).ToArray(), null);

            return method.Invoke(obj, parameters);
        }

        private static FieldInfo GetField(string fieldName, Type objectType)
        {
            FieldInfo field = objectType.GetField(fieldName, Flags);
            if (field == null)
            {
                Type baseType = objectType.BaseType;
                while (baseType != typeof(object))
                {
                    field = GetField(fieldName, baseType);
                    if (field != null)
                    {
                        break;
                    }
                    baseType = baseType.BaseType;
                }
            }
            return field;
        }

        private static PropertyInfo GetProperty(string propertyName, Type objectType)
        {
            PropertyInfo property = objectType.GetProperty(propertyName, Flags);
            if (property == null)
            {
                Type baseType = objectType.BaseType;
                while (baseType != typeof(object))
                {
                    property = GetProperty(propertyName, baseType);
                    if (property != null)
                    {
                        break;
                    }
                    baseType = baseType.BaseType;
                }
            }
            return property;
        }

        public static TProperty PropertyValue<TProperty>(this object source, string propertyName)
        {
            PropertyInfo property = GetProperty(propertyName, source.GetType());

            return (TProperty)property.GetValue(source, Flags, null, null, null);
        }

        public static void SetPropertyValue<TProperty>(this object source, string propertyName, TProperty value)
        {
            PropertyInfo property = GetProperty(propertyName, source.GetType());
            property.SetValue(source, value, Flags, null, null, null);
        }

        public static TField FieldValueEx<TField>(this object source, string fieldName)
        {
            FieldInfo field = GetField(fieldName, source.GetType());

            return (TField)field.GetValue(source);
        }

        public static void SetFieldValue<TField>(this object source, string fieldName, TField value)
        {
            FieldInfo field = GetField(fieldName, source.GetType());

            field.SetValue(source, value);
        }

        public static IDictionary<string, object> ToDictionary(this object @object)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);

            if (@object != null)
            {
                foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(@object))
                    dictionary.Add(property.Name, property.GetValue(@object));
            }

            return dictionary;
        }
        public static T? TryGet<T>(this IDictionary<string, object> values, string key)
            where T : struct
        {
            if (values.TryGetValue(key, out var value) && value is T result)
                return result;

            return null;
        }

        public static object TryGet(this IDictionary<string, object> values, string key)
        {
            values.TryGetValue(key, out var value);
            return value;
        }

        public static TU GetOrDefault<T, TU>(this IDictionary<T, TU> values, T key, TU defaultValue)
        {
            return values.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public static T CoalesceOrDefault<T>(this T self, params T[] values) where T : class
        {
            if (self != null)
                return self;

            for (var index = 0; index < values.Length; index++)
            {
                var value = values[index];
                if (value != null)
                    return value;
            }

            return default(T);
        }

        public static T Coalesce<T>(this T self, params T[] values) where T : class
        {
            if (self != null)
                return self;

            for (var index = 0; index < values.Length; index++)
            {
                var value = values[index];
                if (value != null)
                    return value;
            }

            return null;
        }
    }
}
