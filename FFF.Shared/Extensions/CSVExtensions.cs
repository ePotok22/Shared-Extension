using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace FFF.Shared
{
    public static class CSVExtensions
    {
        public static IEnumerable<string> CSVSplit(this string s)
        {
            CSVSplitState state = CSVSplitState.Normal;
            StringBuilder token = new StringBuilder();
            foreach (char c in s)
            {
                switch (state)
                {
                    case CSVSplitState.Normal:
                        if (c == ',')
                        {
                            yield return token.ToString();
                            token = new StringBuilder();
                        }
                        else if (c == '"')
                            state = CSVSplitState.InQuotes;
                        else
                            token.Append(c);
                        break;

                    case CSVSplitState.InQuotes:
                        if (c == '"')
                            state = CSVSplitState.InQuotesFoundQuote;
                        else
                            token.Append(c);
                        break;

                    case CSVSplitState.InQuotesFoundQuote:
                        if (c == '"')
                        {
                            token.Append(c);
                            state = CSVSplitState.InQuotes;
                        }
                        else
                        {
                            state = CSVSplitState.Normal;
                            goto case CSVSplitState.Normal;
                        }
                        break;
                }
            }
            yield return token.ToString();
        }

        public static string ToCSV<T>(this IEnumerable<T> instance, char separator)
        {
            StringBuilder csv;
            if (instance != null)
            {
                csv = new StringBuilder();
                instance.ForEach(value => csv.AppendFormat("{0}{1}", value, separator));
                return csv.ToString(0, csv.Length - 1);
            }
            return null;
        }

        public static string ToCSV<T>(this IEnumerable<T> instance)
        {
            StringBuilder csv;
            if (instance != null)
            {
                csv = new StringBuilder();
                instance.ForEach(v => csv.AppendFormat("{0},", v));
                return csv.ToString(0, csv.Length - 1);
            }
            return null;
        }

        public static List<string> ToCSV(this IDataReader dataReader, bool includeHeaderAsFirstRow, string separator)
        {
            List<string> csvRows = new List<string>();
            StringBuilder sb = null;

            if (includeHeaderAsFirstRow)
            {
                sb = new StringBuilder();
                for (int index = 0; index < dataReader.FieldCount; index++)
                {
                    if (dataReader.GetName(index) != null)
                        sb.Append(dataReader.GetName(index));

                    if (index < dataReader.FieldCount - 1)
                        sb.Append(separator);
                }
                csvRows.Add(sb.ToString());
            }

            while (dataReader.Read())
            {
                sb = new StringBuilder();
                for (int index = 0; index < dataReader.FieldCount - 1; index++)
                {
                    if (!dataReader.IsDBNull(index))
                    {
                        string value = dataReader.GetValue(index).ToString();
                        if (dataReader.GetFieldType(index) == typeof(String))
                        {
                            //If double quotes are used in value, ensure each are replaced but 2.
                            if (value.IndexOf("\"") >= 0)
                                value = value.Replace("\"", "\"\"");

                            //If separtor are is in value, ensure it is put in double quotes.
                            if (value.IndexOf(separator) >= 0)
                                value = "\"" + value + "\"";
                        }
                        sb.Append(value);
                    }

                    if (index < dataReader.FieldCount - 1)
                        sb.Append(separator);
                }

                if (!dataReader.IsDBNull(dataReader.FieldCount - 1))
                    sb.Append(dataReader.GetValue(dataReader.FieldCount - 1).ToString().Replace(separator, " "));

                csvRows.Add(sb.ToString());
            }
            dataReader.Close();
            sb = null;
            return csvRows;
        }

        public static string ToCSV(this DataTable datatable, char seperator)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < datatable.Columns.Count; i++)
            {
                sb.Append(datatable.Columns[i]);
                if (i < datatable.Columns.Count - 1)
                    sb.Append(seperator);
            }

            sb.AppendLine();

            foreach (DataRow dr in datatable.Rows)
            {
                for (int i = 0; i < datatable.Columns.Count; i++)
                {
                    if (dr[i] is string[])
                    {
                        string[] list = dr[i] as string[];
                        string val = string.Join(";", list);

                        if (val.Contains(","))
                            val = "\"" + val + "\"";

                        sb.Append(val);
                    }
                    else
                    {
                        string value = dr[i].ToString();

                        if (value.Contains("\""))
                            value = value.Replace("\"", "'");

                        if (value.Contains(","))
                            value = "\"" + value + "\"";

                        sb.Append(value);
                    }

                    if (i < datatable.Columns.Count - 1)
                        sb.Append(seperator);
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static string ToCsv<T>(this List<T> list)
        {
            StringBuilder sb = new StringBuilder();

            //Get the properties for type T for the headers
            PropertyInfo[] propInfos = typeof(T).GetProperties();
            for (int i = 0; i <= propInfos.Length - 1; i++)
            {
                sb.Append(propInfos[i].Name);

                if (i < propInfos.Length - 1)
                    sb.Append(",");
            }

            sb.AppendLine();

            //Loop through the collection, then the properties and add the values
            for (int i = 0; i <= list.Count - 1; i++)
            {
                T item = list[i];
                for (int j = 0; j <= propInfos.Length - 1; j++)
                {
                    object o = item.GetType().GetProperty(propInfos[j].Name).GetValue(item, null);
                    if (o != null)
                    {
                        string value = o.ToString();

                        //Check if the value contans a comma and place it in quotes if so
                        if (value.Contains(","))
                            value = string.Concat("\"", value, "\"");

                        //Replace any \r or \n special characters from a new line with a space
                        if (value.Contains("\r"))
                            value = value.Replace("\r", " ");
                        if (value.Contains("\n"))
                            value = value.Replace("\n", " ");
                        sb.Append(value);
                    }

                    if (j < propInfos.Length - 1)
                        sb.Append(",");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
