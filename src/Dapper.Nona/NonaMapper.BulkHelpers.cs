using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Nona
{
    public static partial class NonaMapper
    {
        /// <summary>
        /// The bulk runtime type handle metadata.
        /// </summary>
        private class BulkRuntimeTypeHandleMetadata
        {
            public string Name;
            public PropertyInfo[] PropertyInfos;
            public Tuple<string, Type>[] Columns;
            public Tuple<string, Type> IdentityColumn;
            public string TempTableQuery;
            public string InsertOrUpdateQuery;
            public string UpdateQuery;
            public string DeleteQuery;
        }

        /// <summary>
        /// The bulk operation cache
        /// </summary>
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, BulkRuntimeTypeHandleMetadata> BulkOperationsCache = new ConcurrentDictionary<RuntimeTypeHandle, BulkRuntimeTypeHandleMetadata>();

        /// <summary>
        /// Advanced Settings for SQLBulkCopy class.
        /// </summary>
        /// <param name="bulkcopy">The bulkcopy.</param>
        /// <param name="bulkCopyEnableStreaming">if set to <c>true</c> [bulk copy enable streaming].</param>
        /// <param name="bulkCopyBatchSize">Size of the bulk copy batch.</param>
        /// <param name="bulkCopyNotifyAfter">The bulk copy notify after.</param>
        /// <param name="bulkCopyTimeout">The bulk copy timeout.</param>
        /// <param name="eventHandler">The event handler for copied rows.</param>
        private static void SetSettings(this SqlBulkCopy bulkcopy, bool bulkCopyEnableStreaming, int? bulkCopyBatchSize, int? bulkCopyNotifyAfter, int bulkCopyTimeout, SqlRowsCopiedEventHandler eventHandler)
        {
            bulkcopy.EnableStreaming = bulkCopyEnableStreaming;

            if (bulkCopyBatchSize.HasValue)
            {
                bulkcopy.BatchSize = bulkCopyBatchSize.Value;
            }

            if (bulkCopyNotifyAfter.HasValue)
            {
                bulkcopy.NotifyAfter = bulkCopyNotifyAfter.Value;
            }
            bulkcopy.SqlRowsCopied += eventHandler;
            bulkcopy.BulkCopyTimeout = bulkCopyTimeout;
        }

        /// <summary>
        /// Bulks the copy.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="sqlConnection">The SQL connection.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="dataTable">The data table.</param>
        /// <param name="sqlBulkCopyOptions">The SQL bulk copy options.</param>
        /// <param name="destinationTableName">Name of the destination table.</param>
        /// <param name="bulkCopyTimeout">The bulk copy timeout.</param>
        /// <param name="bulkCopyBatchSize">Size of the bulk copy batch.</param>
        /// <param name="bulkCopyNotifyAfter">The bulk copy notify after.</param>
        /// <param name="bulkCopyEnableStreaming">if set to <c>true</c> [bulk copy enable streaming].</param>
        private static void BulkCopy<TEntity>(
            this SqlConnection sqlConnection,
            SqlTransaction transaction,
            DataTable dataTable,
            SqlBulkCopyOptions sqlBulkCopyOptions,
            string destinationTableName,
            int bulkCopyTimeout,
            int bulkCopyBatchSize,
            int bulkCopyNotifyAfter,
            bool bulkCopyEnableStreaming) where TEntity : class
        {
            using (var bulkCopy = new SqlBulkCopy(sqlConnection, sqlBulkCopyOptions, transaction))
            {
                bulkCopy.DestinationTableName = destinationTableName;
                bulkCopy.SetSettings(bulkCopyEnableStreaming, bulkCopyBatchSize, bulkCopyNotifyAfter, bulkCopyTimeout, rowsCopiedHandler);

                bulkCopy.WriteToServer(dataTable);
            }

            void rowsCopiedHandler(object sender, SqlRowsCopiedEventArgs eventArgs) => LogQuery<TEntity>("Inserted " + eventArgs.RowsCopied + " records.");
        }

        /// <summary>
        /// Bulks the copy asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="sqlConnection">The SQL connection.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="dataTable">The data table.</param>
        /// <param name="sqlBulkCopyOptions">The SQL bulk copy options.</param>
        /// <param name="destinationTableName">Name of the destination table.</param>
        /// <param name="bulkCopyTimeout">The bulk copy timeout.</param>
        /// <param name="bulkCopyBatchSize">Size of the bulk copy batch.</param>
        /// <param name="bulkCopyNotifyAfter">The bulk copy notify after.</param>
        /// <param name="bulkCopyEnableStreaming">if set to <c>true</c> [bulk copy enable streaming].</param>
        /// <returns></returns>
        private static async Task BulkCopyAsync<TEntity>(
            this SqlConnection sqlConnection,
            SqlTransaction transaction,
            DataTable dataTable,
            SqlBulkCopyOptions sqlBulkCopyOptions,
            string destinationTableName,
            int bulkCopyTimeout,
            int bulkCopyBatchSize,
            int bulkCopyNotifyAfter,
            bool bulkCopyEnableStreaming) where TEntity : class
        {
            using (var bulkCopy = new SqlBulkCopy(sqlConnection, sqlBulkCopyOptions, transaction))
            {
                bulkCopy.DestinationTableName = destinationTableName;
                bulkCopy.SetSettings(bulkCopyEnableStreaming, bulkCopyBatchSize, bulkCopyNotifyAfter, bulkCopyTimeout, rowsCopiedHandler);

                await bulkCopy.WriteToServerAsync(dataTable);
            }

            void rowsCopiedHandler(object sender, SqlRowsCopiedEventArgs eventArgs) => LogQuery<TEntity>("Inserted " + eventArgs.RowsCopied + " records.");
        }

        /// <summary>
        /// Gets the temporary data table.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="bulkRuntimeTypeHandleMetadata">The bulk runtime type handle metadata.</param>
        /// <param name="outputIdentity">The output identity.</param>
        /// <returns></returns>
        private static DataTable GetTemporaryDataTable(Type type, out BulkRuntimeTypeHandleMetadata bulkRuntimeTypeHandleMetadata, bool? outputIdentity = null)
        {
            bulkRuntimeTypeHandleMetadata = GetBulkRuntimeTypeHandleMetadata(type, outputIdentity);

            var table = new DataTable(bulkRuntimeTypeHandleMetadata.Name);
            foreach (var info in bulkRuntimeTypeHandleMetadata.Columns)
            {
                table.Columns.Add(new DataColumn(info.Item1, info.Item2));
            }

            return table;
        }

        /// <summary>
        /// Gets the bulk runtime type handle metadata.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="outputIdentity">The output identity.</param>
        /// <returns></returns>
        private static BulkRuntimeTypeHandleMetadata GetBulkRuntimeTypeHandleMetadata(Type type, bool? outputIdentity = null)
        {
            if (BulkOperationsCache.TryGetValue(type.TypeHandle,
                out var bulkRuntimeTypeHandleMetadata))
            {
                return bulkRuntimeTypeHandleMetadata;
            }

            bulkRuntimeTypeHandleMetadata = new BulkRuntimeTypeHandleMetadata();
            var tableName = Resolvers.Table(type);
            var keyProperty = Resolvers.KeyProperty(type, out var isIdentity);

            var columns = new List<Tuple<string, Type>>();
            var propertyInfos = new List<PropertyInfo>();

            foreach (var typeProperty in Resolvers.Properties(type))
            {
                if (typeProperty == keyProperty)
                {
                    if (isIdentity)
                    {
                        bulkRuntimeTypeHandleMetadata.IdentityColumn = Resolvers.DataColumn(typeProperty);
                    }
                }
                if (typeProperty.PropertyInfo.GetSetMethod() != null)
                {
                    columns.Add(Resolvers.DataColumn(typeProperty));
                    propertyInfos.Add(typeProperty.PropertyInfo);
                }
            }

            if (outputIdentity.HasValue && outputIdentity.Value)
            {
                columns.Add(new Tuple<string, Type>("InternalId", typeof(int)));
                propertyInfos.Add(null);
            }

            bulkRuntimeTypeHandleMetadata.Columns = columns.ToArray();
            bulkRuntimeTypeHandleMetadata.PropertyInfos = propertyInfos.ToArray();
            bulkRuntimeTypeHandleMetadata.Name = tableName;

            BulkOperationsCache.TryAdd(type.TypeHandle, bulkRuntimeTypeHandleMetadata);

            return bulkRuntimeTypeHandleMetadata;
        }

        /// <summary>
        /// Loads a DataTable from a sequence of objects.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="table">The input table. The schema of the table must match that
        /// the type T.  If the table is null, a new table is created with a schema
        /// created from the public properties and fields of the type T.</param>
        /// <param name="source">The sequence of objects to load into the DataTable.</param>
        /// <param name="bulkRuntimeTypeHandleMetadata">The bulk runtime type handle metadata.</param>
        /// <param name="options">Specifies how values from the source sequence will be applied to
        /// existing rows in the table.</param>
        /// <param name="outputIdentityDic">The output identity dic.</param>
        /// <param name="identifierOnly">if set to <c>true</c> [identifier only].</param>
        /// <returns>
        /// A DataTable created from the source sequence.
        /// </returns>
        private static DataTable Shred<TEntity>(this DataTable table, IEnumerable<TEntity> source, BulkRuntimeTypeHandleMetadata bulkRuntimeTypeHandleMetadata, LoadOption? options, Dictionary<int, TEntity> outputIdentityDic = null, bool identifierOnly = false) where TEntity : class
        {
            var counter = 0;
            // Enumerate the source sequence and load the object values into rows.
            table.BeginLoadData();
            using (var e = source.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    if (options != null)
                    {
                        table.LoadDataRow(ShredObject(bulkRuntimeTypeHandleMetadata, e.Current, counter, outputIdentityDic, identifierOnly), (LoadOption)options);
                    }
                    else
                    {
                        table.LoadDataRow(ShredObject(bulkRuntimeTypeHandleMetadata, e.Current, counter, outputIdentityDic, identifierOnly), true);
                    }
                    counter++;
                }
            }
            table.EndLoadData();

            // Return the table.
            return table;
        }

        /// <summary>
        /// Shreds the object.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="bulkRuntimeTypeHandleMetadata">The bulk runtime type handle metadata.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="counter">The counter.</param>
        /// <param name="outputIdentityDic">The output identity dic.</param>
        /// <param name="identifierOnly">if set to <c>true</c> [identifier only].</param>
        /// <returns></returns>
        private static object[] ShredObject<TEntity>(BulkRuntimeTypeHandleMetadata bulkRuntimeTypeHandleMetadata, TEntity instance, int counter, Dictionary<int, TEntity> outputIdentityDic, bool identifierOnly) where TEntity : class
        {
            var length = bulkRuntimeTypeHandleMetadata.Columns.Length;

            var values = new object[length];

            var index = 0;
            foreach (var pInfo in bulkRuntimeTypeHandleMetadata.PropertyInfos)
            {
                if (bulkRuntimeTypeHandleMetadata.Columns[index].Item1 == "InternalId")
                {
                    values[index++] = counter;
                    outputIdentityDic.Add(counter, instance);
                }
                if (identifierOnly)
                {
                    values[index++] = bulkRuntimeTypeHandleMetadata.Columns[index].Item1 == bulkRuntimeTypeHandleMetadata.IdentityColumn.Item1 ? pInfo.GetValue(instance, null) : null;
                }
                else values[index++] = pInfo.GetValue(instance, null);
            }

            return values;
        }

        /// <summary>
        /// Checks the temporary table query.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="bulkRuntimeTypeHandleMetadata">The bulk runtime type handle metadata.</param>
        /// <param name="outputIdentity">The output identity.</param>
        private static void CheckTemporaryTableQuery(IDbConnection connection, IDbTransaction transaction, BulkRuntimeTypeHandleMetadata bulkRuntimeTypeHandleMetadata, bool? outputIdentity = null)
        {
            if (!string.IsNullOrEmpty(bulkRuntimeTypeHandleMetadata.TempTableQuery))
            {
                return;
            }

            var internalCol = string.Empty;
            if (outputIdentity.HasValue && outputIdentity.Value)
            {
                internalCol = ", [InternalId] int";
            }

            var tempTableGetCreateQuery = $@"
                SELECT N'CREATE TABLE #{bulkRuntimeTypeHandleMetadata.Name}(' + Stuff((
	                SELECT N', [' + c.name + '] ' + CASE 
			                WHEN t.Name in ('varchar', 'nvarchar', 'char', 'binary', 'varbinary')
			                THEN IIF(c.max_length = '-1', CONVERT(nvarchar(10),t.Name) + '(max)', CONVERT(nvarchar(10),t.Name) + '(' + CONVERT(nvarchar(10),c.max_length) + ')')
			                WHEN t.Name in ('numeric', 'decimal')
			                THEN CONVERT(nvarchar(10),t.Name) + '(' + CONVERT(nvarchar(10),c.precision) + ', ' + CONVERT(nvarchar(10),c.scale) + ')'
			                ELSE t.Name
		                END	
	                FROM sys.columns c INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
	                LEFT OUTER JOIN sys.index_columns ic ON ic.object_id = c.object_id AND ic.column_id = c.column_id
	                LEFT OUTER JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
	                WHERE c.object_id = OBJECT_ID('{bulkRuntimeTypeHandleMetadata.Name}')	FOR XML PATH(''), TYPE).value('text()[1]','nvarchar(max)'),1,2,N'') + '{internalCol});'";

            var result = connection.ExecuteScalar<string>(tempTableGetCreateQuery, transaction: transaction);

            bulkRuntimeTypeHandleMetadata.TempTableQuery = result;
        }

        /// <summary>
        /// Checks the update table query.
        /// </summary>
        /// <param name="bulkMetadata">The bulk metadata.</param>
        private static void CheckUpdateTableQuery(BulkRuntimeTypeHandleMetadata bulkMetadata)
        {
            if (!string.IsNullOrEmpty(bulkMetadata.UpdateQuery))
                return;

            string query = $@"MERGE INTO {bulkMetadata.Name} WITH (HOLDLOCK) AS Target 
                              USING #{bulkMetadata.Name} AS Source 
                              {BuildJoinConditionsForUpdateOrInsert(bulkMetadata)} 
                              WHEN MATCHED THEN {BuildUpdateSet(bulkMetadata)}; 
                              DROP TABLE #{bulkMetadata.Name};";

            bulkMetadata.UpdateQuery = query;
        }

        /// <summary>
        /// Checks the insert or update table query.
        /// </summary>
        /// <param name="bulkMetadata">The bulk metadata.</param>
        private static void CheckInsertOrUpdateTableQuery(BulkRuntimeTypeHandleMetadata bulkMetadata)
        {
            if (!string.IsNullOrEmpty(bulkMetadata.InsertOrUpdateQuery))
                return;
            
            string query = $@"MERGE INTO {bulkMetadata.Name} WITH (HOLDLOCK) AS Target 
                            USING #{bulkMetadata.Name} AS Source 
                            {BuildJoinConditionsForUpdateOrInsert(bulkMetadata)} 
                            WHEN MATCHED THEN {BuildUpdateSet(bulkMetadata)}
                            WHEN NOT MATCHED BY TARGET THEN {BuildInsertSet(bulkMetadata)}";

            bulkMetadata.InsertOrUpdateQuery = query;
        }

        /// <summary>
        /// Checks the delete table query.
        /// </summary>
        /// <param name="bulkMetadata">The bulk metadata.</param>
        private static void CheckDeleteTableQuery(BulkRuntimeTypeHandleMetadata bulkMetadata)
        {
            if (!string.IsNullOrEmpty(bulkMetadata.DeleteQuery))
                return;

            string query = $@"MERGE INTO {bulkMetadata.Name} WITH (HOLDLOCK) AS Target 
                            USING #{bulkMetadata.Name} AS Source 
                            {BuildJoinConditionsForUpdateOrInsert(bulkMetadata)} 
                            WHEN MATCHED THEN DELETE; DROP TABLE #{bulkMetadata.Name};";

            bulkMetadata.DeleteQuery = query;
        }

        /// <summary>
        /// Builds the join conditions for update or insert.
        /// </summary>
        /// <param name="bulkMetadata">The bulk metadata.</param>
        /// <returns></returns>
        private static string BuildJoinConditionsForUpdateOrInsert(BulkRuntimeTypeHandleMetadata bulkMetadata) => 
            $"ON {bulkMetadata.Name}.[{bulkMetadata.IdentityColumn.Item1}] = #{bulkMetadata.Name}.[{bulkMetadata.IdentityColumn.Item1}] ";

        /// <summary>
        /// Builds the update set.
        /// </summary>
        /// <param name="bulkMetadata">The bulk metadata.</param>
        /// <returns></returns>
        private static string BuildUpdateSet(BulkRuntimeTypeHandleMetadata bulkMetadata)
        {
            StringBuilder command = new StringBuilder();
            List<string> paramsSeparated = new List<string>();

            command.Append("UPDATE SET ");

            foreach (var column in bulkMetadata.Columns)
            {
                if ((bulkMetadata.IdentityColumn != null && column != bulkMetadata.IdentityColumn) || bulkMetadata.IdentityColumn == null)
                {
                    if (column.Item1 != "InternalId")
                        paramsSeparated.Add($"{bulkMetadata.Name}.[{column.Item1 }] = #{bulkMetadata.Name}.[{column.Item1 }]");
                }
            }

            command.Append(string.Join(", ", paramsSeparated) + " ");

            return command.ToString();
        }

        /// <summary>
        /// Builds the insert set.
        /// </summary>
        /// <param name="bulkMetadata">The bulk metadata.</param>
        /// <returns></returns>
        private static string BuildInsertSet(BulkRuntimeTypeHandleMetadata bulkMetadata)
        {
            StringBuilder command = new StringBuilder();
            List<string> insertColumns = new List<string>();

            command.Append("INSERT (");

            foreach (var column in bulkMetadata.Columns)
            {
                if ((bulkMetadata.IdentityColumn != null && column != bulkMetadata.IdentityColumn) || bulkMetadata.IdentityColumn == null)
                {
                    if (column.Item1 != "InternalId")
                        insertColumns.Add($"[{column.Item1 }]");
                }
            }

            command.Append(string.Join(", ", insertColumns));
            command.Append(") values (");
            command.Append(string.Join(", ", insertColumns));
            command.Append(")");

            return command.ToString();
        }
    }
}
