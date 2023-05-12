using System.Collections.Generic;
using System.Data;

namespace FFF.Shared
{
    public static class DataColumnExtensions
    {
        public static IEnumerable<DataColumn> AsEnumerable(this DataColumnCollection source)
        {
            if (source != null)
            {
                foreach (DataColumn itemColumn in source)
                    yield return itemColumn;
            }
        }

        public static void AddRange(this DataColumnCollection source, IList<string> columns)
        {
            foreach (string column in columns)
                source.Add(column);
        }
    }
}
