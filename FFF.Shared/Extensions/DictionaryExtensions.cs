using System;
using System.Collections.Generic;
using System.Text;

namespace FFF.Shared
{
    public static class DictionaryExtensions
    {
        public static string ToString<TKey1, TKey2>(this Dictionary<TKey1, TKey2> source)
        {
            StringBuilder stringBuilder = new StringBuilder();
            IEnumerator<KeyValuePair<TKey1, TKey2>> tempEnumerator = source.GetEnumerator();

            while (tempEnumerator.MoveNext())
            {
                if (string.IsNullOrWhiteSpace(stringBuilder.ToString()))
                {
                    if (tempEnumerator.Current.Value != null)
                        stringBuilder.Append($"{tempEnumerator.Current.Key} : {tempEnumerator.Current.Value}");
                    else
                        stringBuilder.Append($"{tempEnumerator.Current.Key} :");
                }
                else
                {
                    if (tempEnumerator.Current.Value != null)
                        stringBuilder.Append($"{Environment.NewLine}{tempEnumerator.Current.Key} : {tempEnumerator.Current.Value}");
                    else
                        stringBuilder.Append($"{Environment.NewLine}{tempEnumerator.Current.Key} :");
                }
            }
            return stringBuilder.ToString();
        }

        public static bool TryRemove<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, out TValue value)
        {
            bool isRemoved = dictionary.TryGetValue(key, out value);
            if (isRemoved)
                dictionary.Remove(key);

            return isRemoved;
        }

        public static TValue AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value, Func<TKey, TValue, TValue> updateValueFactory)
        {
            TValue oldValue;
            if (dictionary.TryGetValue(key, out oldValue))
            {
                // TRY / CATCH should be done here, but this application does not require it
                value = updateValueFactory(key, oldValue);
                dictionary[key] = value;
            }
            else
                dictionary.Add(key, value);

            return value;
        }

        public static TValue AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            TValue value;
            TValue oldValue;
            if (dictionary.TryGetValue(key, out oldValue))
            {
                value = updateValueFactory(key, oldValue);
                dictionary[key] = value;
            }
            else
            {
                value = addValueFactory(key);
                dictionary.Add(key, value);
            }

            return value;
        }

        public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, value);

            return true;
        }

    }
}
