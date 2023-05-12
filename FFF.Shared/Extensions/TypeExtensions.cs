using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace FFF.Shared
{
    public static class TypeExtensions
    {

        public static bool IsFileInUse(this IOException ioEx) => ioEx != null && ioEx.HResult == -2147024864;

        public static Type FindTypeInPropertyName(this Type type, IList<string> names)
        {
            if (names?.Count() > 0)
            {
                if ((type.GetInterface(typeof(ICollection).Name) != null) ||
                    (type.GetInterface(typeof(IEnumerable).Name) != null))
                {
                    DefaultMemberAttribute tempDMA = type.GetCustomAttribute<DefaultMemberAttribute>();
                    if (tempDMA != null)
                    {
                        PropertyInfo tempPropertyInfo = type.GetProperties().Where(o => tempDMA.MemberName.StartsWith(o.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        if (tempPropertyInfo != null)
                        {
                            return FindTypeInPropertyName(tempPropertyInfo.PropertyType, names);
                        }
                    }
                }
                else
                {
                    string tempName = names.First();
                    PropertyInfo temp = type.GetProperties().Where(o => tempName.StartsWith(o.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (temp != null)
                    {
                        if (names?.Count() == 1)
                        {
                            Regex myRegex = new Regex("\\[[\"-A-Za-z0-9\\-]+]", RegexOptions.IgnoreCase);
                            MatchCollection matches = myRegex.Matches(tempName.Replace(temp.Name, string.Empty));
                            if (matches.Count > 1)
                            {
                                if ((temp.PropertyType.GetInterface(typeof(ICollection).Name) != null) ||
                                    (temp.PropertyType.GetInterface(typeof(IEnumerable).Name) != null))
                                {
                                    DefaultMemberAttribute tempDMA = temp.PropertyType.GetCustomAttribute<DefaultMemberAttribute>();
                                    PropertyInfo tempPropertyFirst = temp.PropertyType.GetProperties().Where(o => tempDMA.MemberName.StartsWith(o.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                                    if (tempPropertyFirst != null)
                                    {
                                        PropertyInfo tempPropertySecord = tempPropertyFirst.PropertyType.GetProperties().Where(o => tempDMA.MemberName.StartsWith(o.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                                        return tempPropertySecord.PropertyType;
                                    }
                                }
                            }
                            return temp.PropertyType;
                        }
                        return FindTypeInPropertyName(temp.PropertyType, names.Skip(1).ToArray());
                    }
                }
            }
            return null;
        }

        public static Type FindInterface(this Type type, Type ifaceType) => type.ImplementInterface(ifaceType);

        public static bool IsList(this Type type)
        {
            if (type == null) return false;
            return IsInterfaceOf(type, typeof(IList)) &&
                   type.IsGenericType &&
                   type.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        public static bool IsCollection(this Type type)
        {
            if (type == null) return false;
            return IsInterfaceOf(type, typeof(ICollection)) &&
                   type.IsGenericType &&
                   type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Collection<>));
        }

        public static bool IsDictionary(this Type type)
        {
            if (type == null) return false;
            return IsInterfaceOf(type, typeof(System.Collections.IDictionary)) &&
                   type.IsGenericType &&
                   type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>));
        }

        public static bool IsInterfaceOf(this Type type, Type ifaceType) => type.ImplementInterface(ifaceType) != null;

        public static string GetFriendlyName(this Type type, bool fullName = false, bool omitGenericArguments = false)
        {
            string str = fullName ? type.FullName : type.Name;
            if (!type.IsGenericType)
                return str;
            int length = str.IndexOf('`');
            if (length > 0)
                str = str.Substring(0, length);
            if (omitGenericArguments)
                return str;
            Type[] genericArguments = type.GetGenericArguments();
            StringBuilder stringBuilder = new StringBuilder(str);
            stringBuilder.Append("<");
            for (int index = 0; index < genericArguments.Length - 1; ++index)
                stringBuilder.AppendFormat("{0},", (object)genericArguments[index].GetFriendlyName(), (object)fullName);
            stringBuilder.AppendFormat("{0}>", (object)genericArguments[genericArguments.Length - 1].GetFriendlyName(), (object)fullName);
            return stringBuilder.ToString();
        }

        internal static Type ImplementInterface(this Type type, Type ifaceType)
        {
            while (type != null)
            {
                Type[] interfaces = type.GetInterfaces();
                if (interfaces != null)
                {
                    for (int i = 0; i < interfaces.Length; i++)
                    {
                        if (interfaces[i] == ifaceType || (interfaces[i] != null && interfaces[i].ImplementInterface(ifaceType) != null))
                        {
                            return interfaces[i];
                        }
                    }
                }

                type = type.BaseType;
            }

            return default;
        }

        public static bool IsNumeric(this Type type)
        {
            if (type.IsSubclassOfRawGeneric(typeof(Nullable<>)))
                type = ((IEnumerable<Type>)type.GenericTypeArguments).First<Type>();
            if (type.IsEnum)
                return false;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsNullableNumeric(this Type type) =>
            type.IsSubclassOfRawGeneric(typeof(Nullable<>)) && ((IEnumerable<Type>)type.GenericTypeArguments).First<Type>().IsNumeric();

        public static string CleanInvalidXmlChars(this string text, string replaceWith = "")
        {
            string pattern = "[^\\x09\\x0A\\x0D\\x20-\\uD7FF\\uE000-\\uFFFD\\u10000-\\u10FFFF]";
            return Regex.Replace(text, pattern, replaceWith);
        }

        public static string Description(this Type activity)
        {
            IEnumerable<DescriptionAttribute> source = TypeDescriptor.GetAttributes(activity).OfType<DescriptionAttribute>();
            return source.Count<DescriptionAttribute>() > 0 ? source.ElementAt<DescriptionAttribute>(0).Description : (string)null;
        }

        public static string DisplayName(this Type activity)
        {
            DisplayNameAttribute displayNameAttribute = TypeDescriptor.GetAttributes(activity).OfType<DisplayNameAttribute>().FirstOrDefault<DisplayNameAttribute>();
            if (displayNameAttribute != null)
                return displayNameAttribute.DisplayName;
            return activity.IsGenericType && !activity.IsGenericTypeDefinition ? activity.GetGenericTypeDefinition().DisplayName() : (string)null;
        }

        public static string HelpKeyword(this Type activity)
        {
            IEnumerable<HelpKeywordAttribute> source = TypeDescriptor.GetAttributes(activity).OfType<HelpKeywordAttribute>();
            return source.Count<HelpKeywordAttribute>() > 0 ? source.ElementAt<HelpKeywordAttribute>(0).HelpKeyword : (string)null;
        }

        public static bool IsExpandableProperty(this Type propertyType)
        {
            try
            {
                TypeConverterAttribute converterAttribute = TypeDescriptor.GetAttributes(propertyType).OfType<TypeConverterAttribute>().FirstOrDefault<TypeConverterAttribute>();
                if (converterAttribute == null)
                    return false;
                Type type = Type.GetType(converterAttribute.ConverterTypeName);
                if (type != (Type)null)
                    return type.IsAssignableFrom(typeof(ExpandableObjectConverter)) || type.IsSubclassOf(typeof(ExpandableObjectConverter));
            }
            catch (Exception ex)
            {
                ex.Trace();
            }
            return false;
        }

        public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic)
        {
            for (; toCheck != (Type)null && toCheck != typeof(object); toCheck = toCheck.BaseType)
            {
                Type type = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == type)
                    return true;
            }
            return false;
        }

        public static bool IsSameOrSubclass(this Type toCheck, Type potentialBase)
        {
            if (toCheck == (Type)null || potentialBase == (Type)null)
                return false;
            return toCheck.IsSubclassOf(potentialBase) || toCheck == potentialBase;
        }

        public static Type GetAsGenericTypeOrDefault(this Type type) =>
            !type.IsGenericType || type.IsGenericTypeDefinition ? type : type.GetGenericTypeDefinition();

        public static Type GetUnderlyingTypeFromGenericTypeOrDefault(this Type type)
        {
            if (type == (Type)null)
                return (Type)null;
            return !type.IsGenericType || type.IsGenericTypeDefinition ? type : ((IEnumerable<Type>)type.GenericTypeArguments).First<Type>();
        }

        public static Type GetUnderlyingTypeFromGenericOrArray(this Type type)
        {
            if (type == (Type)null)
                return (Type)null;
            return type.IsArray ? type.GetElementType() : type.GetUnderlyingTypeFromGenericTypeOrDefault();
        }

        public static IEnumerable<Type> GetUsedTypes(this Type targetType)
        {
            return internalGetUsedTypes(targetType).Distinct<Type>();

            IEnumerable<Type> internalGetUsedTypes(Type type)
            {
                Type typeAsGeneric = type.GetAsGenericTypeOrDefault();
                yield return typeAsGeneric;
                Type[] typeArray = type.GenericTypeArguments;
                for (int index = 0; index < typeArray.Length; ++index)
                {
                    foreach (Type usedType in typeArray[index].GetUsedTypes())
                    {
                        Type genericTypeOrDefault = usedType.GetAsGenericTypeOrDefault();
                        if (!usedType.IsGenericParameter && typeAsGeneric != genericTypeOrDefault)
                            yield return genericTypeOrDefault;
                    }
                }
                typeArray = (Type[])null;
            }
        }

        public static HashSet<Type> GetUsedTypesFullHierarchy(this Type targetType)
        {
            HashSet<Type> usedTypes = new HashSet<Type>();
            targetType.WalkParents(usedTypes);
            return usedTypes;
        }

        private static void WalkParents(this Type type, HashSet<Type> usedTypes)
        {
            if (type == (Type)null || type == typeof(object) || type == typeof(string) || type.IsPrimitive)
                return;
            Type genericTypeOrDefault = type.GetAsGenericTypeOrDefault();
            if (usedTypes.Contains(genericTypeOrDefault))
                return;
            usedTypes.Add(genericTypeOrDefault);
            TypeExtensions.WalkParentsAndGenericArgumentTypes(type, usedTypes);
            foreach (Type type1 in type.GetInterfaces())
                TypeExtensions.WalkParentsAndGenericArgumentTypes(type1, usedTypes);
            for (Type baseType = type.BaseType; baseType != (Type)null && baseType != typeof(object); baseType = baseType.BaseType)
                TypeExtensions.WalkParentsAndGenericArgumentTypes(baseType, usedTypes);
        }

        private static void WalkParentsAndGenericArgumentTypes(Type type, HashSet<Type> usedTypes)
        {
            foreach (Type usedType in type.GetUsedTypes())
                usedType.WalkParents(usedTypes);
        }

        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == (Assembly)null)
                throw new ArgumentNullException(nameof(assembly));
            try
            {
                return (IEnumerable<Type>)assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ((IEnumerable<Type>)ex.Types).Where<Type>((Func<Type, bool>)(t => t != (Type)null));
            }
        }

        public static System.Attribute[] ToAttributeArray(object[] objects) =>
            objects == null ? new System.Attribute[0] : objects.Cast<System.Attribute>().ToArray<System.Attribute>();

        public static bool IsNullableType(this Type type)
        {
            if (type == (Type)null)
                return false;
            return Nullable.GetUnderlyingType(type) != (Type)null;
        }

        public static string GetSafeFullName(this Type type, bool fullyQualifiedAssemblyName)
        {
            if (type == (Type)null)
                return "NullType";
            string safeFullName = type.FullName == null ? type.Name : type.FullName;
            if (fullyQualifiedAssemblyName)
            {
                string str = "unknown_assembly";
                Assembly assemblyEx = type.Assembly;
                if (assemblyEx != (Assembly)null)
                    str = assemblyEx.FullName;
                safeFullName = safeFullName + ", " + str;
            }
            return safeFullName;
        }

        /// <summary>
        /// Converts Enumeration type into a dictionary of names and values
        /// </summary>
        /// <param name="t">Enum type</param>
        public static IDictionary<string, int> EnumToDictionary(this Type t)
        {
            if (t == null) throw new NullReferenceException();
            if (!t.IsEnum) throw new InvalidCastException("object is not an Enumeration");

            string[] names = Enum.GetNames(t);
            Array values = Enum.GetValues(t);

            return (from i in Enumerable.Range(0, names.Length)
                    select new { Key = names[i], Value = (int)values.GetValue(i) })
                        .ToDictionary(k => k.Key, k => k.Value);
        }

        public static string GetDisplayName(this Type input)
        {
            string displayName = input.Name;

            for (int i = displayName.Length - 1; i >= 0; i--)
            {
                if (displayName[i] == char.ToUpper(displayName[i]))
                    if (i > 0)
                        displayName = displayName.Insert(i, " ");
            }
            return displayName;
        }

    }
}
