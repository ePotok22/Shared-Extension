using System;
using System.Collections.Concurrent;
using System.Linq;

namespace FFF.Shared
{
    public static class ConcurrentDictionaryExtensions
    {
        public static bool ContainsKeyIgnoringCase<T>(this ConcurrentDictionary<string, T> dictionary, string desiredKeyOfAnyCase) =>
            GetKeyIgnoringCase(dictionary, desiredKeyOfAnyCase) != null;

        public static string GetKeyIgnoringCase<T>(this ConcurrentDictionary<string, T> dictionary, string desiredKeyOfAnyCase) =>
            dictionary.FirstOrDefault(a => a.Key.Equals(desiredKeyOfAnyCase, StringComparison.OrdinalIgnoreCase)).Key;

        public static bool TryGetValueIgnoringCase<T>(this ConcurrentDictionary<string, T> dictionary, string desiredKeyOfAnyCase, out T value)
        {
            string key = GetKeyIgnoringCase(dictionary, desiredKeyOfAnyCase);
            if (key != null)
                return dictionary.TryGetValue(key, out value);
            else
            {
                value = default;
                return false;
            }
        }
    }
}
