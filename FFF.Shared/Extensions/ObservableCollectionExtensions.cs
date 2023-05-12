using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
namespace FFF.Shared
{
    public static class ObservableCollectionExtensions
    {
        public static void AddRange<T>(this ObservableCollection<T> oc, IEnumerable<T> collection) 
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            foreach (var item in collection)
                oc.Add(item);

        }

        public static void Sort<TSource, TKey>(this ObservableCollection<TSource> collection, Func<TSource, TKey> func)
        {
            List<TSource> sorted = Enumerable.OrderBy<TSource, TKey>(collection, func).ToList();
            for (int i = 0; i < sorted.Count(); i++)
                collection.Move(collection.IndexOf(sorted[i]), i);
        }

        public static void Reverse<TSource, TKey>(this ObservableCollection<TSource> collection, Func<TSource, TKey> func)
        {
            List<TSource> sorted = Enumerable.OrderByDescending<TSource, TKey>(collection, func).ToList();
            for (int i = 0; i < sorted.Count(); i++)
                collection.Move(collection.IndexOf(sorted[i]), i);
        }
    }
}
