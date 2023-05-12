using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FFF.Shared
{
    /// <summary>
    /// This static class defines the DataTable extension methods.
    /// </summary>
    public static class DataTableExtensions
    {
        public static bool HasDataRow(this DataTable source)
        {
            ValidateNullDT(source);

            return source.Rows.Count > 0;
        }

        public static bool HasDataColumn(this DataTable source)
        {
            ValidateNullDT(source);

            return source.Columns.Count > 0;
        }

        public static DataTable CloneData(this DataTable source)
        {
            ValidateNullDT(source);

            DataTable tempResult = source.Clone();
            tempResult.BeginLoadData();

            foreach (DataRow itemRow in source.Rows)
                tempResult.LoadDataRow(itemRow.ItemArray, false);

            tempResult.EndLoadData();
            return tempResult;
        }

        /// <summary>
        /// This method convert datatable to pivot dataTable
        /// </summary>
        public static DataTable ToPivotTable<T, TColumn, TRow, TData>(this IEnumerable<T> source
            , Func<T, TColumn> columnSelector
            , Expression<Func<T, TRow>> rowSelector
            , Func<IEnumerable<T>, TData> dataSelector)
        {
            DataTable table = new DataTable();
            string rowName = ((MemberExpression)rowSelector.Body).Member.Name;
            table.Columns.Add(new DataColumn(rowName));
            IEnumerable<TColumn> columns = source.Select(columnSelector).Distinct();

            foreach (TColumn column in columns)
                table.Columns.Add(new DataColumn(column.ToString()));

            var rows = source.GroupBy(rowSelector.Compile())
                             .Select(rowGroup => new
                             {
                                 Key = rowGroup.Key,
                                 Values = columns.GroupJoin(
                                     rowGroup,
                                     c => c,
                                     r => columnSelector(r),
                                     (c, columnGroup) => dataSelector(columnGroup))
                             });

            foreach (var row in rows)
            {
                DataRow dataRow = table.NewRow();
                List<object> items = row.Values.Cast<object>().ToList();
                items.Insert(0, row.Key);
                dataRow.ItemArray = items.ToArray();
                table.Rows.Add(dataRow);
            }

            return table;
        }

        /// <summary>
        /// Force Value To String, then return DataTable out.
        /// </summary>
        public static DataTable ToDataTableForceOutValueString(this DataTable table)
        {
            if (table == null)
                return null;

            DataTable temp = table.Clone();
            foreach (DataColumn column in temp.Columns)
                column.DataType = typeof(string);

            temp.BeginLoadData();
            IEnumerator tempGetEnumerator = table.Rows.GetEnumerator();
            while (tempGetEnumerator.MoveNext())
            {
                object[] current = ((DataRow)tempGetEnumerator.Current)?.ItemArray;
                temp.LoadDataRow(current?.Select(item => item?.ToString()).ToArray<object>() ?? Array.Empty<object>(), false);
            }
            temp.EndLoadData();
            return temp;
        }

        public static string ToString(this DataTable source)
        {
            if (source != null)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append($"Table Name : {source.TableName}");
                if (source.Columns.Count > 0)
                {
                    stringBuilder.Append($"{Environment.NewLine}Columns : ");
                    foreach (DataColumn item in source.Columns)
                        stringBuilder.Append($", {item.ColumnName}");
                }
                else
                    stringBuilder.Append($"{Environment.NewLine}Not have column!");
                if (source.Rows.Count > 0)
                {
                    int count = 1;
                    foreach (DataRow item in source.Rows)
                    {
                        stringBuilder.Append($"{Environment.NewLine}Rows[{count}] : ");
                        foreach (object itemIn in item.ItemArray)
                            stringBuilder.Append($", {itemIn}");
                    }
                }
                else
                    stringBuilder.Append($"{Environment.NewLine}Not have row!");
                return stringBuilder.ToString();
            }
            else
                return string.Empty;
        }

        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        public static List<T> ToList<T>(this DataTable table) where T : class, new()
        {
            try
            {
                List<T> list = new List<T>();
                foreach (DataRow row in table.AsEnumerable())
                {
                    T obj = new T();
                    foreach (PropertyInfo prop in obj.GetType().GetProperties())
                    {
                        try
                        {
                            PropertyInfo propertyInfo = obj.GetType().GetProperty(prop.Name);
                            propertyInfo.SetValue(obj, Convert.ChangeType(row[prop.Name], propertyInfo.PropertyType), null);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    list.Add(obj);
                }
                return list;
            }
            catch
            {
                return null;
            }
        }

        public static void ValidateNullDT(this DataTable source)
        {
            if (source == null)
                throw new NullReferenceException("DataTable is be null");
        }
    }
}
