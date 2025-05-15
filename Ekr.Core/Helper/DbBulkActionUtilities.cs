using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Ekr.Core.Helper
{
    public static class DbBulkActionUtilities
    {
        public static void BulkInsert<T>(string connectionString, string tableName, IList<T> list)
        {
            using var bulkCopy = new SqlBulkCopy(connectionString);
            bulkCopy.BatchSize = list.Count;
            bulkCopy.DestinationTableName = tableName;

            var table = new DataTable();
            var props = TypeDescriptor.GetProperties(typeof(T))
                                       //Dirty hack to make sure we only have system data types
                                       //i.e. filter out the relationships/collections
                                       .Cast<PropertyDescriptor>()
                                       .Where(propertyInfo => propertyInfo.PropertyType.Namespace.Equals("System"))
                                       .ToArray();

            foreach (var propertyInfo in props)
            {
                bulkCopy.ColumnMappings.Add(propertyInfo.Name, propertyInfo.Name);
                table.Columns.Add(propertyInfo.Name, Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType);
            }

            var values = new object[props.Length];
            foreach (var item in list)
            {
                for (var i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }

                table.Rows.Add(values);
            }

            try
            {
                bulkCopy.WriteToServer(table);
            }
            catch (SqlException ex)
            {
                const string pattern = @"\d+";
                Match match = Regex.Match(ex.Message, pattern);
                var index = Convert.ToInt32(match.Value) - 1;

                FieldInfo fi = typeof(SqlBulkCopy).GetField("_sortedColumnMappings", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fi != null)
                {
                    var sortedColumns = fi.GetValue(bulkCopy);
                    var items = (Object[])sortedColumns.GetType().GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sortedColumns);

                    FieldInfo itemdata = items[index].GetType().GetField("_metadata", BindingFlags.NonPublic | BindingFlags.Instance);
                    var metadata = itemdata.GetValue(items[index]);

                    var column = metadata.GetType().GetField("column", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(metadata);
                    var length = metadata.GetType().GetField("length", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(metadata);
                    throw new OperationCanceledException(String.Format("Column: {0} contains data with a length greater than: {1}", column, length));
                }
            }
        }
    }
}
