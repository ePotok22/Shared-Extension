using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace FFF.Shared
{
    public static class AssemblyNameCacheExtensions
    {
        private static readonly ConcurrentDictionary<string, string> _assemblyShortNames = new ConcurrentDictionary<string, string>();

        public static string GetCachedShortName(this Assembly assembly) =>
            !(assembly == (Assembly)null) ? _assemblyShortNames.GetOrAdd(assembly.FullName, _ => assembly.GetName().Name) : (string)null;

        public static void Invalidate() =>
            _assemblyShortNames.Clear();
    }
}
