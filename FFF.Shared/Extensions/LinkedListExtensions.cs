using System;
using System.Collections.Generic;

namespace FFF.Shared
{
    public static class LinkedListExtensions
    {
        public static void RemoveAll<T>(this LinkedList<T> linkedList, Func<T, bool> predicate)
        {
            LinkedListNode<T> next;
            for (LinkedListNode<T> node = linkedList.First; node != null; node = next)
            {
                next = node.Next;
                if (predicate(node.Value))
                    linkedList.Remove(node);
            }
        }
    }
}
