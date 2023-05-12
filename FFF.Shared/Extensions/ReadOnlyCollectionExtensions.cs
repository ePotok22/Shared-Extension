using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FFF.Shared
{
    public static class ReadOnlyCollectionExtensions
    {
        public static IReadOnlyCollection<T> AsReadOnly<T>(this IList<T> list) =>
            list == null ? (IReadOnlyCollection<T>)null : (IReadOnlyCollection<T>)new ReadOnlyCollection<T>(list);
    }
}
