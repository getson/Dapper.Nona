using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dapper.Nona.Abstractions;

namespace Dapper.Nona
{
    public static partial class NonaMapper
    {
        private struct DataTableInfo
        { 
            public string Name;
            public DataColumn[] Columns;
            public PropertyInfo[] PropertyInfos;
        }

        private static readonly ConcurrentDictionary<Type, DataTableInfo> BulkInsertQueryCache = new ConcurrentDictionary<Type, DataTableInfo>();

        /// <summary>
        /// Inserts the specified entities into the database and returns the id.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="entities">The entity to be inserted.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <returns>
        /// The id of the inserted entity.
        /// </returns>
        public static void BulkInsert<T>(this IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction = null) where T : class
        {
            var dataTable = GetBulkInsertDataTable(typeof(T), out var dataTableInfo).Shred(entities, dataTableInfo, null);
            LogQuery<T>(dataTable.DisplayExpression);

            using (var bulkCopy = new SqlBulkCopy((SqlConnection)connection))
            {
                bulkCopy.DestinationTableName = dataTableInfo.Name;
                bulkCopy.WriteToServer(dataTable);
            }
        }

        /// <summary>
        /// Inserts the specified entity into the database and returns the id.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="entities">The entity to be inserted.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <returns>The id of the inserted entity.</returns>
        public static Task BulkInsertAsync<T>(this IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction = null) where T : class
        {
            var dataTable = GetBulkInsertDataTable(typeof(T), out var dataTableInfo).Shred(entities, dataTableInfo, null);
            LogQuery<T>(dataTable.DisplayExpression);

            Task output;
            using (var bulkCopy = new SqlBulkCopy((SqlConnection)connection))
            {
                bulkCopy.DestinationTableName = dataTableInfo.Name;
                output = bulkCopy.WriteToServerAsync(dataTable);
            }

            return output;
        }

        private static DataTable GetBulkInsertDataTable(Type type, out DataTableInfo dataTableInfo)
        {
            if (!BulkInsertQueryCache.TryGetValue(type, out dataTableInfo))
            {
                var tableName =Resolvers.Table(type);
                var keyProperty =Resolvers.KeyProperty(type, out var isIdentity);

                var typeProperties = new List<NonaProperty>();
                foreach (var typeProperty in Resolvers.Properties(type))
                {
                    if (typeProperty == keyProperty)
                    {
                        if (isIdentity)
                        {
                            // Skip key properties marked as an identity column.
                            continue;
                        }
                    }

                    if (typeProperty.PropertyInfo.GetSetMethod() != null)
                    {
                        typeProperties.Add(typeProperty);
                    }
                }

                dataTableInfo = new DataTableInfo()
                {
                    Columns = typeProperties.Select(Resolvers.DataColumn).ToArray(),
                    PropertyInfos = typeProperties.Select(p => p.PropertyInfo).ToArray(),
                    Name = tableName
                };

                BulkInsertQueryCache.TryAdd(type, dataTableInfo);
            }

            var table = new DataTable(dataTableInfo.Name);
            table.Columns.AddRange(dataTableInfo.Columns);
            return table;
        }

        /// <summary>
        /// Loads a DataTable from a sequence of objects.
        /// </summary>
        /// <param name="source">The sequence of objects to load into the DataTable.</param>
        /// <param name="table">The input table. The schema of the table must match that 
        /// the type T.  If the table is null, a new table is created with a schema 
        /// created from the public properties and fields of the type T.</param>
        /// <param name="tableInfo"></param>
        /// <param name="options">Specifies how values from the source sequence will be applied to 
        /// existing rows in the table.</param>
        /// <returns>A DataTable created from the source sequence.</returns>
        private static DataTable Shred<T>(this DataTable table, IEnumerable<T> source, DataTableInfo tableInfo, LoadOption? options) where T : class
        {
            // Enumerate the source sequence and load the object values into rows.
            table.BeginLoadData();
            using (var e = source.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    if (options != null)
                    {
                        table.LoadDataRow(ShredObject(tableInfo, e.Current), (LoadOption)options);
                    }
                    else
                    {
                        table.LoadDataRow(ShredObject(tableInfo, e.Current), true);
                    }
                }
            }
            table.EndLoadData();

            // Return the table.
            return table;
        }

        private static object[] ShredObject<T>(DataTableInfo tableInfo, T instance) where T : class
        {
            var values = new object[tableInfo.Columns.Length];
            var index = 0;
            foreach (var pInfo in tableInfo.PropertyInfos)
            {
                values[index++] = pInfo.GetValue(instance, null);
            }

            // Return the property and field values of the instance.
            return values;
        }
    }
}
