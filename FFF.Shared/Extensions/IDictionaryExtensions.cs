using System;
using System.Collections.Generic;
using System.Linq;

namespace FFF.Shared
{
    public static class IDictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, TValue ifNotFound = default(TValue))
        {
            TValue val;
            return self.TryGetValue(key, out val) ? val : ifNotFound;
        }

        public static TValue GetOrAddThreadSafe<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, Func<TKey, TValue> factory)
        {
            TValue tValue;
            TValue tValue1;

            lock (self)
            {
                if (!self.TryGetValue(key, out tValue))
                {
                    tValue = factory(key);
                    self[key] = tValue;
                }

                tValue1 = tValue;
            }

            return tValue1;
        }

        public static bool ContainsKeyWithValue<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, params TValue[] values)
        {
            if (self == null || values == null || values.Length == 0)
                return false;

            TValue temp;
            try
            {
                if (!self.TryGetValue(key, out temp))
                    return false;
            }
            catch (ArgumentNullException)
            {
                return false;
            }

            return values.Any(v => v.Equals(temp));
        }

        public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, value);

            return true;
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

        public static TU GetOrDefault<T, TU>(this IDictionary<T, TU> values, T key, TU defaultValue) =>
             values.TryGetValue(key, out var value) ? value : defaultValue;
    }
}
