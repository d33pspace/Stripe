using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Stripe
{
    public class EnumInfo<T>
    {
        public static string GetDescription(T value)
        {
            var fieldInfo = TypeExtensions.GetField(value.GetType(), value.ToString());
            var attributes =
                (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
        }


        public static string GetDescription<AttributeType>(T value)
        {
            var fieldInfo = TypeExtensions.GetField(value.GetType(), value.ToString());
            var itbs = from item in fieldInfo.GetCustomAttributes(typeof(AttributeType), false)
                select item.ToString();

            var enumerable = itbs as string[] ?? itbs.ToArray();
            return (enumerable.Any()) ? enumerable.First() : value.ToString();
        }

        public static List<KeyValuePair<T, string>> GetValues()
        {
            return (from T enumValue in Enum.GetValues(typeof(T)) select new KeyValuePair<T, string>(enumValue, GetDescription(enumValue))).ToList();
        }

        public static List<KeyValuePair<T, string>> GetValues<AttributeType>()
        {
            return (from T enumValue in Enum.GetValues(typeof(T)) select new KeyValuePair<T, string>(enumValue, GetDescription<AttributeType>(enumValue))).ToList();
        }
    }
}