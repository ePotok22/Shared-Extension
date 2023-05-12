using System;
using System.Collections;
using System.Collections.Generic;

namespace FFF.Shared
{
    public static class IListExtensions
    {
        public static void RemoveFirst(this IList list)
        {
            if (list.Count == 0)
                return;
            list.RemoveAt(0);
        }

        public static void RemoveLast(this IList list)
        {
            if (list.Count == 0)
                return;
            list.RemoveAt(list.Count - 1);
        }

        public static void InsertRange<T>(this IList<T> list, int index, IEnumerable<T> toInsert)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            if (toInsert == null)
                throw new ArgumentNullException(nameof(toInsert));
            if (index < 0 || index > list.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (list is List<T> objList)
                objList.InsertRange(index, toInsert);
            else
            {
                int num = index;
                foreach (T obj in toInsert)
                    list.Insert(num++, obj);
            }
        }
    }
}
