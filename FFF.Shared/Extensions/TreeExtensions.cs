using System;
using System.Collections.Generic;
using System.Linq;

namespace FFF.Shared
{
    public static class TreeExtensions
    {
        public static IEnumerable<Tuple<T, int>> FlattenWithLevel<T>(
          this IEnumerable<T> items,
          Func<T, IEnumerable<T>> getChildren)
        {
            IEnumerable<T> source = items;
            if ((source != null ? (!source.Any<T>() ? 1 : 0) : 1) == 0)
            {
                Stack<Tuple<T, int>> stack = new Stack<Tuple<T, int>>();
                foreach (T obj in items)
                    stack.Push(new Tuple<T, int>(obj, 1));
                while (stack.Count > 0)
                {
                    Tuple<T, int> current = stack.Pop();
                    yield return current;
                    foreach (T obj in getChildren(current.Item1))
                        stack.Push(new Tuple<T, int>(obj, current.Item2 + 1));
                    current = (Tuple<T, int>)null;
                }
            }
        }
    }
}
