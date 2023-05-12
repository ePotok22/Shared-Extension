using System;
using System.Collections.Generic;
using System.Linq;

namespace FFF.Shared
{
    public static class ArrayExtensions
    {
        public static T[] Page<T>(this T[] source, int page, int pageSize) =>
            IEnumerableExtensions.Page(source, page, pageSize).ToArray();

        public static T[] RemoveAt<T>(this T[] source, int index)
        {
            if (source?.Length > 0)
            {
                if (index <= (source.Length - 1))
                {
                    for (int a = index; a < source.Length - 1; a++)
                        // moving elements downwards, to fill the gap at [index]
                        source[a] = source[a + 1];
                    // finally, let's decrement Array's size by one
                    Array.Resize(ref source, source.Length - 1);
                }
            }

            return source;
        }

        public static T[] RemoveRange<T>(this T[] source, int[] index)
        {
            if (source?.Length > 0 && index?.Length > 0)
            {
                for (int a = 0; a < index.Length; a++)
                    source = RemoveAt(source, a);
            }
            return source;
        }

        public static List<T> ToList<T>(this Array items, Func<object, T> mapFunction)
        {
            if (items == null || mapFunction == null)
                return new List<T>();

            List<T> coll = new List<T>();
            for (int i = 0; i < items.Length; i++)
            {
                T val = mapFunction(items.GetValue(i));
                if (val != null)
                    coll.Add(val);
            }
            return coll;
        }

        /// <summary>
        /// Determines if an array is null or empty.
        /// </summary>
        /// <param name="obj">The array to check.</param>
        /// <returns>True if null or empty, false otherwise.</returns>
        public static bool IsNullOrEmpty(this Array obj) =>
            (obj == null) || (obj.Length == 0);

        /// <summary>
        /// Determines if an array is not null or empty.
        /// </summary>
        /// <param name="obj">The array to check.</param>
        /// <returns>True if not null or empty, false otherwise.</returns>
        public static bool NonEmpty(this Array obj) =>
            obj != null && obj.Length > 0;

        /// <summary>
        /// Shuffles an array of objects.
        /// </summary>
        /// <typeparam name="T">The type of the array to sort.</typeparam>
        /// <param name="array">The array to sort.</param>
        public static void Shuffle<T>(this T[] array)
        {
            int length = array.Length;
            Random random = new Random();
            while (length > 1)
            {
                int randomNumber = random.Next(length--);
                T obj = array[length];
                array[length] = array[randomNumber];
                array[randomNumber] = obj;
            }
        }

    }
}
