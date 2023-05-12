using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace FFF.Shared
{
    public static class DataRowExtensions
    {
        public static IEnumerable<DataRow> AsEnumerable(this DataRowCollection source)
        {
            if (source != null)
            {
                foreach (DataRow item in source)
                    yield return item;
            }
        }

        public static IEnumerable<TKey> AsEnumerable<TKey>(this DataRow source)
        {
            if (source != null)
            {
                foreach (object item in source.ItemArray)
                {
                    if (item.Equals(DBNull.Value))
                        yield return (TKey)default;
                    else
                        yield return (TKey)Convert.ChangeType(item, typeof(TKey));
                }
            }
        }

        public static bool EqualsColumn(this DataRow source, string text)
        {
            if (source != null && !string.IsNullOrWhiteSpace(text))
            {
                foreach (DataColumn itemColumn in source.Table.Columns)
                {
                    string temp = GetField<string>(source, itemColumn.ColumnName.Trim('\r', '\n'));
                    if (temp?.Equals(text?.Trim(), StringComparison.OrdinalIgnoreCase) ?? false)
                        return true;
                }
            }
            return false;
        }

        public static DataColumn FindColumn(this DataRow source, string column)
        {
            if (source != null)
            {
                DataTable tempData = source.Table;
                for (int i = 0; i < tempData.Columns.Count; i++)
                {
                    if (tempData.Columns[i].ColumnName.Trim('\r', '\n').Equals(column.Trim(), StringComparison.OrdinalIgnoreCase))
                        return tempData.Columns[i];
                }
            }
            return null;
        }

        public static Type GetFieldType(this DataRow source, string column)
        {
            if (source != null)
            {
                DataTable tempData = source.Table;
                for (int i = 0; i < tempData.Columns.Count; i++)
                {
                    if (tempData.Columns[i].ColumnName.Trim('\r', '\n').Equals(column.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        if (source[tempData.Columns[i].Ordinal].Equals(DBNull.Value))
                            break;
                        else
                            return tempData.Columns[i].DataType;
                    }
                }
            }
            return null;
        }

        public static TKey GetField<TKey>(this DataRow source, string column)
        {
            if (source != null)
            {
                DataTable tempData = source.Table;
                for (int i = 0; i < tempData.Columns.Count; i++)
                {
                    if (tempData.Columns[i].ColumnName.Trim('\r', '\n').Equals(column.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        if (source[tempData.Columns[i].Ordinal].Equals(DBNull.Value))
                            break;
                        else
                            return (TKey)Convert.ChangeType(source[tempData.Columns[i].Ordinal], typeof(TKey));
                    }
                }
            }
            return (TKey)default;
        }

        public static void SetField<TKey>(this DataRow source, string column, TKey value)
        {
            if (source != null)
                source.MapField(column, value);
        }

        public static DataRow SetUp(this DataRow source, string[] columns, object[] values)
        {
            if (source != null)
            {
                if (columns == null)
                    return source;
                for (int i = 0; i < columns.Length; i++)
                    source.MapField(columns[i], i < values?.Length ? values[i] : DBNull.Value);
            }
            return source;
        }

        public static DataTable ToDataTable<TKey>(this IEnumerable<TKey> source) where TKey : DataRow
        {
            if (source == null) return null;
            else if (!source.Any()) return null;

            DataTable tempResult = null;
            bool isColumn = false;

            foreach (DataRow itemRow in source)
            {
                if (isColumn.Equals(false))
                {
                    isColumn = !isColumn;
                    tempResult = itemRow.Table.Clone();
                }

                DataRow tempRow = tempResult.NewRow();
                tempRow.ItemArray = itemRow.ItemArray;
                tempResult.Rows.Add(tempRow);
            }

            return tempResult;
        }

        private static void MapField<TKey>(this DataRow source, string column, TKey value)
        {
            if (source != null)
            {
                DataTable tempData = source.Table;
                for (int i = 0; i < tempData.Columns.Count; i++)
                {
                    if (tempData.Columns[i].ColumnName.Trim('\r', '\n').Equals(column.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        source[tempData.Columns[i].Ordinal] = (object)value ?? DBNull.Value;
                        break;
                    }
                }
            }

        }
    }
}
