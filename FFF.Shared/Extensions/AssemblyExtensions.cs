using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace FFF.Shared
{
    public static class AssemblyExtensions
    {
        public static bool IsBrowsable(this Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            BrowsableAttribute attribute;

            return !assembly.TryGetAttribute<BrowsableAttribute>(out attribute) || attribute.Browsable;
        }

        private static bool TryGetAttribute<T>(this Assembly assembly, out T attribute) where T : System.Attribute
        {
            attribute = assembly.GetCustomAttributes(typeof(T), false).Cast<T>().FirstOrDefault();
            return (object)attribute != null;
        }

        /// <summary>
        /// Loads the configuration from assembly attributes
        /// </summary>
        /// <typeparam name="T">The type of the custom attribute to find.</typeparam>
        /// <param name="callingAssembly">The calling assembly to search.</param>
        /// <returns>The custom attribute of type T, if found.</returns>
        public static T GetAttribute<T>(this Assembly callingAssembly) where T : System.Attribute
        {
            T result = null;

            // Try to find the configuration attribute for the default logger if it exists
            object[] configAttributes = Attribute.GetCustomAttributes(callingAssembly,
                    typeof(T), false);

            // get just the first one
            if (configAttributes != null)
                result = (T)configAttributes[0];

            return result;
        }
    }
}
