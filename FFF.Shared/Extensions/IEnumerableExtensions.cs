using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;

namespace FFF.Shared
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<string> Trim(this IEnumerable<string> source)
        {
            if (source != null)
                return source.Select(item => item?.Trim());
            return default;
        }

        public static IEnumerable<TSource> RemoveEmptyNull<TSource>(this IEnumerable<TSource> source)
        {
            if (source != null)
            {
                return source.Where(item =>
                {
                    if (item != null)
                    {
                        if (item.GetType() == typeof(string))
                        {
                            if (string.IsNullOrWhiteSpace(item.ToString()))
                                return false;
                        }
                        return true;
                    }
                    else
                        return false;
                });
            }
            return default;
        }

        public static IEnumerable<TSource> Page<TSource>(this IEnumerable<TSource> source, int page, int pageSize)
        {
            if (source != null)
                return source.Skip((page - 1) * pageSize).Take(pageSize);
            return default;
        }

        public static IEnumerable<T> Distinct<T, TKey>(this IEnumerable<T> @this, Func<T, TKey> keySelector) =>
            @this.GroupBy(keySelector).Select(grps => grps).Select(e => e.First());

        public static IEnumerable<TSource> WhereIf<TSource>(this IEnumerable<TSource> source, bool condition, Func<TSource, bool> predicate)
        {
            if (condition)
                return source.Where(predicate);
            else
                return source;
        }

        public static IEnumerable<TSource> WhereIf<TSource>(this IEnumerable<TSource> source, bool condition, Func<TSource, int, bool> predicate)
        {
            if (condition)
                return source.Where(predicate);
            else
                return source;
        }

        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> list, string sortExpression)
        {
            sortExpression += "";
            string[] parts = sortExpression.Split(' ');
            bool descending = false;
            string property = "";

            if (parts.Length > 0 && parts[0] != "")
            {
                property = parts[0];
                if (parts.Length > 1)
                    descending = parts[1].IndexOf("esc", StringComparison.OrdinalIgnoreCase) >= 0;

                PropertyInfo prop = typeof(T).GetProperty(property);

                if (prop == null)
                    throw new Exception("No property '" + property + "' in + " + typeof(T).Name + "'");

                if (descending)
                    return list.OrderByDescending(x => prop.GetValue(x, null));
                else
                    return list.OrderBy(x => prop.GetValue(x, null));
            }

            return list;
        }

        /// <summary>
        /// Converts an enumeration of groupings into a Dictionary of those groupings.
        /// </summary>
        /// <typeparam name="TKey">Key type of the grouping and dictionary.</typeparam>
        /// <typeparam name="TValue">Element type of the grouping and dictionary list.</typeparam>
        /// <param name="groupings">The enumeration of groupings from a GroupBy() clause.</param>
        /// <returns>A dictionary of groupings such that the key of the dictionary is TKey type and the value is List of TValue type.</returns>
        public static Dictionary<TKey, List<TValue>> ToDictionary<TKey, TValue>(this IEnumerable<IGrouping<TKey, TValue>> groupings) =>
             groupings.ToDictionary(group => group.Key, group => group.ToList());

        public static Dictionary<TFirstKey, Dictionary<TSecondKey, TValue>> Pivot<TSource, TFirstKey, TSecondKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TFirstKey> firstKeySelector, Func<TSource, TSecondKey> secondKeySelector, Func<IEnumerable<TSource>, TValue> aggregate)
        {
            var retVal = new Dictionary<TFirstKey, Dictionary<TSecondKey, TValue>>();

            ILookup<TFirstKey, TSource> l = source.ToLookup(firstKeySelector);
            foreach (IGrouping<TFirstKey, TSource> item in l)
            {
                var dict = new Dictionary<TSecondKey, TValue>();
                retVal.Add(item.Key, dict);
                ILookup<TSecondKey, TSource> subdict = item.ToLookup(secondKeySelector);
                foreach (IGrouping<TSecondKey, TSource> subitem in subdict)
                    dict.Add(subitem.Key, aggregate(subitem));
            }

            return retVal;
        }

        public static IEnumerable<T> AsForEach<T>(this IEnumerable<T> array, Action<T> act)
        {
            foreach (var i in array)
                act(i);

            return array;
        }

        public static IEnumerable<RT> AsForEach<T, RT>(this IEnumerable<T> array, Func<T, RT> func)
        {
            List<RT> list = new List<RT>();
            foreach (var i in array)
            {
                var obj = func(i);
                if (obj != null)
                    list.Add(obj);
            }
            return list;
        }

        public static void ForEach<TItem>(this IEnumerable<TItem> collection, Action<TItem> action)
        {
            if (collection == null)
                return;
            foreach (TItem obj in collection)
                action(obj);
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> coll)
        {
            var c = new ObservableCollection<T>();
            foreach (var e in coll)
                c.Add(e);
            return c;
        }

        public static IEnumerable<t> Randomize<t>(this IEnumerable<t> target)
        {
            Random r = new Random();

            return target.OrderBy(x => (r.Next()));
        }

        public static Collection<T> ToCollection<T>(this IEnumerable<T> enumerable)
        {
            var collection = new Collection<T>();
            foreach (T i in enumerable)
                collection.Add(i);
            return collection;
        }

        public static IEnumerable<IEnumerable<T>> Transpose<T>(this IEnumerable<IEnumerable<T>> values)
        {
            if (values.Count() == 0)
                return values;
            if (values.First().Count() == 0)
                return Transpose(values.Skip(1));

            var x = values.First().First();
            var xs = values.First().Skip(1);
            var xss = values.Skip(1);
            return
             new[] {new[] {x}
           .Concat(xss.Select(ht => ht.First()))}
               .Concat(new[] { xs }
               .Concat(xss.Select(ht => ht.Skip(1)))
               .Transpose());
        }

        /// <summary>
        /// Returns the index of the first occurrence in a sequence by using the default equality comparer.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="list">A sequence in which to locate a value.</param>
        /// <param name="value">The object to locate in the sequence</param>
        /// <returns>The zero-based index of the first occurrence of value within the entire sequence, if found; otherwise, –1.</returns>
        public static int IndexOf<TSource>(this IEnumerable<TSource> list, TSource value) where TSource : IEquatable<TSource>
        {
            return list.IndexOf<TSource>(value, EqualityComparer<TSource>.Default);

        }

        /// <summary>
        /// Returns the index of the first occurrence in a sequence by using a specified IEqualityComparer.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="list">A sequence in which to locate a value.</param>
        /// <param name="value">The object to locate in the sequence</param>
        /// <param name="comparer">An equality comparer to compare values.</param>
        /// <returns>The zero-based index of the first occurrence of value within the entire sequence, if found; otherwise, –1.</returns>
        public static int IndexOf<TSource>(this IEnumerable<TSource> list, TSource value, IEqualityComparer<TSource> comparer)
        {
            int index = 0;
            foreach (var item in list)
            {
                if (comparer.Equals(item, value))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> list)
        {
            var r = new Random((int)DateTime.Now.Ticks);
            var shuffledList = list.Select(x => new { Number = r.Next(), Item = x }).OrderBy(x => x.Number).Select(x => x.Item);
            return shuffledList.ToList();
        }

        public static IEnumerable<T> Slice<T>(this IEnumerable<T> collection, int start, int end)
        {
            int index = 0;
            int count = 0;

            if (collection == null)
                throw new ArgumentNullException("collection");

            // Optimise item count for ICollection interfaces.
            if (collection is ICollection<T>)
                count = ((ICollection<T>)collection).Count;
            else if (collection is ICollection)
                count = ((ICollection)collection).Count;
            else
                count = collection.Count();

            // Get start/end indexes, negative numbers start at the end of the collection.
            if (start < 0)
                start += count;

            if (end < 0)
                end += count;

            foreach (var item in collection)
            {
                if (index >= end)
                    yield break;

                if (index >= start)
                    yield return item;

                ++index;
            }
        }

        /// <summary>
		/// Converts an IEnumerable to a HashSet
		/// </summary>
		/// <typeparam name="T">The IEnumerable type</typeparam>
		/// <param name="enumerable">The IEnumerable</param>
		/// <returns>A new HashSet</returns>
		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable)
        {
            HashSet<T> hs = new HashSet<T>();
            foreach (T item in enumerable)
                hs.Add(item);
            return hs;
        }
        public static IEnumerable<T> Cache<T>(this IEnumerable<T> source) =>
            CacheHelper(source.GetEnumerator());

        private static IEnumerable<T> CacheHelper<T>(IEnumerator<T> source)
        {
            var isEmpty = new Lazy<bool>(() => !source.MoveNext());
            var head = new Lazy<T>(() => source.Current);
            var tail = new Lazy<IEnumerable<T>>(() => CacheHelper(source));

            return CacheHelper(isEmpty, head, tail);
        }

        private static IEnumerable<T> CacheHelper<T>(Lazy<bool> isEmpty, Lazy<T> head, Lazy<IEnumerable<T>> tail)
        {
            if (isEmpty.Value)
                yield break;

            yield return head.Value;
            foreach (var value in tail.Value)
                yield return value;
        }

        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int splitSize)
        {
            using (IEnumerator<T> enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                    yield return InnerSplit(enumerator, splitSize);
            }

        }

        private static IEnumerable<T> InnerSplit<T>(IEnumerator<T> enumerator, int splitSize)
        {
            int count = 0;
            do
            {
                count++;
                yield return enumerator.Current;
            }
            while (count % splitSize != 0
                 && enumerator.MoveNext());
        }

        public static IEnumerable<T> Filter<T>(this IEnumerable<T> list, Func<T, bool> filterParam)
        {
            return list.Where(filterParam);
        }

        public static double StdDevP(this IEnumerable<int> source)
        {
            return StdDevLogic(source, 0);
        }

        public static double StdDev(this IEnumerable<int> source)
        {
            return StdDevLogic(source, 1);
        }

        private static double StdDevLogic(this IEnumerable<int> source, int buffer = 1)
        {
            if (source == null)
            { throw new ArgumentNullException("source"); }

            var data = source.ToList();
            var average = data.Average();
            var differences = data.Select(u => Math.Pow(average - u, 2.0)).ToList();
            return Math.Sqrt(differences.Sum() / (differences.Count() - buffer));
        }

        /// <summary>
        /// Implementation of Scala's ZipWithIndex method.
        ///
        /// Folds a collection into a Dictionary where the original value (of type T) acts as the key
        /// and the index of the item in the array acts as the value.
        /// </summary>
        /// <typeparam name="T">TBD</typeparam>
        /// <param name="collection">TBD</param>
        /// <returns>TBD</returns>
        public static Dictionary<T, int> ZipWithIndex<T>(this IEnumerable<T> collection)
        {
            var i = 0;
            var dict = new Dictionary<T, int>();
            foreach (var item in collection)
            {
                dict.Add(item, i);
                i++;
            }
            return dict;
        }

        /// <summary>
        /// Select all the items in this array beginning with <paramref name="startingItem"/> and up until the end of the array.
        ///
        /// <note>
        /// If <paramref name="startingItem"/> is not found in the array, From will return an empty set.
        /// If <paramref name="startingItem"/> is found at the end of the array, From will return the entire original array.
        /// </note>
        /// </summary>
        /// <typeparam name="T">TBD</typeparam>
        /// <param name="items">TBD</param>
        /// <param name="startingItem">TBD</param>
        /// <returns>TBD</returns>
        internal static IEnumerable<T> From<T>(this IEnumerable<T> items, T startingItem)
        {
            var itemsAsList = items.ToList();
            var indexOf = itemsAsList.IndexOf(startingItem);
            if (indexOf == -1) return new List<T>();
            if (indexOf == 0) return itemsAsList;
            var itemCount = (itemsAsList.Count - indexOf);
            return itemsAsList.Slice(indexOf, itemCount);
        }

        /// <summary>
        /// Select all the items in this array from the beginning until (but not including) <paramref name="startingItem"/>
        /// <note>
        /// If <paramref name="startingItem"/> is not found in the array, Until will select all items.
        /// If <paramref name="startingItem"/> is the first item in the array, an empty array will be returned.
        /// </note>
        /// </summary>
        /// <typeparam name="T">TBD</typeparam>
        /// <param name="items">TBD</param>
        /// <param name="startingItem">TBD</param>
        /// <returns>TBD</returns>
        internal static IEnumerable<T> Until<T>(this IEnumerable<T> items, T startingItem)
        {
            var enumerator = items.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                if (Equals(current, startingItem))
                    yield break;
                yield return current;
            }
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <typeparam name="T">TBD</typeparam>
        /// <param name="items">TBD</param>
        /// <returns>TBD</returns>
        internal static IEnumerable<T> Tail<T>(this IEnumerable<T> items)
        {
            return items.Skip(1);
        }

        [Description("Listing 12.17")]
        public static T RandomElement<T>(this IEnumerable<T> source, Random random)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (random == null)
                throw new ArgumentNullException("random");

            ICollection collection = source as ICollection;
            if (collection != null)
            {
                int count = collection.Count;
                if (count == 0)
                    throw new InvalidOperationException("Sequence was empty.");
                int index = random.Next(count);
                return source.ElementAt(index);
            }

            ICollection<T> genericCollection = source as ICollection<T>;
            if (genericCollection != null)
            {
                int count = genericCollection.Count;
                if (count == 0)
                    throw new InvalidOperationException("Sequence was empty.");
                int index = random.Next(count);
                return source.ElementAt(index);
            }

            using (IEnumerator<T> iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                    throw new InvalidOperationException("Sequence was empty.");
                int countSoFar = 1;
                T current = iterator.Current;
                while (iterator.MoveNext())
                {
                    countSoFar++;
                    if (random.Next(countSoFar) == 0)
                        current = iterator.Current;
                }
                return current;
            }
        }
    }
}
