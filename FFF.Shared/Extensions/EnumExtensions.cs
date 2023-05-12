using System;
using System.ComponentModel;
using System.Reflection;

namespace FFF.Shared
{
    public static class EnumExtensions
    {
        private static TKey[] FindCustomAttributes<TKey>(Enum @enum)
        {
            Type tempType = @enum.GetType();
            FieldInfo tempField = tempType.GetField(@enum.ToString());
            return tempField.GetCustomAttributes(typeof(TKey), false) as TKey[];
        }

        public static string GetName(this Enum @enum)
        {
            Type tempType = @enum.GetType();
            return Enum.GetName(tempType, @enum);
        }

        public static string GetDescriptionOfValue(this Enum @enum)
        {
            ValueInfomationAttribute[] tempDescription = FindCustomAttributes<ValueInfomationAttribute>(@enum);
            if (tempDescription.Length > 0)
                return tempDescription[0].Description;
            return null;
        }

        public static TKey GetValueInfomation<TKey>(this Enum @enum, TypeInformation selectInformation)
        {
            ValueInfomationAttribute[] tempDescription = FindCustomAttributes<ValueInfomationAttribute>(@enum);
            object tempValue = null;
            if (tempDescription.Length > 0)
            {
                switch (selectInformation)
                {
                    case TypeInformation.Description:
                        tempValue = tempDescription[0].Description as object;
                        break;
                    case TypeInformation.Value:
                        tempValue = tempDescription[0].Value as object;
                        break;
                }
            }
            else
                tempValue = @enum as object;
            tempValue = Convert.ChangeType(tempValue, typeof(TKey));
            return (TKey)tempValue;
        }

        public static string GetDescription(this Enum enumVal) => 
            enumVal.GetAttributeOfType<DescriptionAttribute>()?.Description;

        private static T GetAttributeOfType<T>(this Enum enumVal) where T : System.Attribute
        {
            object[] customAttributes = enumVal.GetType().GetMember(enumVal.ToString())[0].GetCustomAttributes(typeof(T), false);
            return customAttributes.Length == 0 ? default(T) : (T)customAttributes[0];
        }
        public static string GetTranslation(this Enum x)
        {
            DescriptionAttribute[] customAttributes = (DescriptionAttribute[])x.GetType().GetField(x.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return customAttributes != null && !string.IsNullOrEmpty(customAttributes[0].Description) ? customAttributes[0].Description : x.ToString();
        }
    }
}
