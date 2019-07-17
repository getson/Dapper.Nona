using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.Nona
{
    public static partial class NonaMapper
    {
        /// <summary>
        /// Inserts or updates the specified entities into the database.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="entities">The entity to be inserted.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="sqlBulkCopyOptions">The SQL bulk copy options.</param>
        /// <param name="bulkCopyTimeout">The bulk copy timeout.</param>
        /// <param name="bulkCopyBatchSize">Size of the bulk copy batch.</param>
        /// <param name="bulkCopyNotifyAfter">The bulk copy notify after.</param>
        /// <param name="bulkCopyEnableStreaming">if set to <c>true</c> [bulk copy enable streaming].</param>
        /// <param name="deleteWhenNotMatched">if set to <c>true</c> [delete when not matched].</param>
        /// <param name="outputIdentity">The output identity.</param>
        public static void BulkInsertOrUpdate<TEntity>(
            this IDbConnection connection,
            IEnumerable<TEntity> entities,
            IDbTransaction transaction = null,
            SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.Default,
            int bulkCopyTimeout = 600,
            int bulkCopyBatchSize = 5000,
            int bulkCopyNotifyAfter = 1000,
            bool bulkCopyEnableStreaming = false,
            bool deleteWhenNotMatched = false,
            bool? outputIdentity = null
            ) where TEntity : class
        {
            if (!entities.Any())
            {
                return;
            }

            var outputIdentityDic = outputIdentity.HasValue && outputIdentity.Value ?
                new Dictionary<int, TEntity>() : null;

            var dataTable = GetTemporaryDataTable(typeof(TEntity), out var bulkMetadata, outputIdentity).Shred(entities, bulkMetadata, null, outputIdentityDic);

            var sqlConnection = (SqlConnection)connection;

            var command = sqlConnection.CreateCommand();
            command.Connection = sqlConnection;
            var sqlTransaction = (SqlTransaction)transaction;
            command.Transaction = sqlTransaction;
            command.CommandTimeout = 600;
            CheckTemporaryTableQuery(connection, transaction, bulkMetadata, true);
            //Creating temp table on database
            command.CommandText = bulkMetadata.TempTableQuery;
            command.ExecuteNonQuery();
            //Bulk copy into temp table
            BulkCopy<TEntity>(sqlConnection, sqlTransaction, dataTable, sqlBulkCopyOptions,
                        bulkMetadata.Name, bulkCopyTimeout, bulkCopyBatchSize, bulkCopyNotifyAfter, bulkCopyEnableStreaming);
            // Updating destination table, and dropping temp table
            CheckInsertOrUpdateTableQuery(bulkMetadata);

            var outputCreateTableQuery = outputIdentity.HasValue && outputIdentity.Value ?
                $"CREATE TABLE #{bulkMetadata.Name}InsertOutput(InternalId int, Id int); " : "";
            var outputIdentityQuery = outputIdentity.HasValue && outputIdentity.Value ?
                $"OUTPUT Source.InternalId, INSERTED.{bulkMetadata.IdentityColumn.Item1} INTO #{bulkMetadata.Name}InsertOutput(InternalId, {bulkMetadata.IdentityColumn.Item1}); " : ";";
            var deleteWhenNotMatchedQuery = deleteWhenNotMatched ?
                " WHEN NOT MATCHED BY SOURCE THEN DELETE " : " ";
            var query = $"{outputCreateTableQuery} {bulkMetadata.UpdateQuery} {deleteWhenNotMatchedQuery} {outputIdentityQuery} DROP TABLE #{bulkMetadata.Name};";

            command.CommandText = query;
            command.ExecuteNonQuery();

            if (outputIdentity.HasValue && outputIdentity.Value)
            {
                command.CommandText = $"SELECT InternalId, Id FROM #{bulkMetadata.Name}InsertOutput;";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (outputIdentityDic.TryGetValue((int)reader[0], out var item))
                        {
                            var type = item.GetType();
                            var prop = type.GetProperty(bulkMetadata.IdentityColumn.Item1);
                            prop.SetValue(item, reader[1], null);
                        }
                    }
                }
            }

            command.CommandText = $"DROP TABLE #{bulkMetadata.Name}InsertOutput;";
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Inserts or updates the specified entities into the database async.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="entities">The entity to be inserted.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="sqlBulkCopyOptions">The SQL bulk copy options.</param>
        /// <param name="bulkCopyTimeout">The bulk copy timeout.</param>
        /// <param name="bulkCopyBatchSize">Size of the bulk copy batch.</param>
        /// <param name="bulkCopyNotifyAfter">The bulk copy notify after.</param>
        /// <param name="bulkCopyEnableStreaming">if set to <c>true</c> [bulk copy enable streaming].</param>
        /// <param name="deleteWhenNotMatched">if set to <c>true</c> [delete when not matched].</param>
        /// <param name="outputIdentity">The output identity.</param>
        /// <returns>
        /// The id of the inserted entity.
        /// </returns>
        public static async Task BulkInsertOrUpdateAsync<TEntity>(
            this IDbConnection connection,
            IEnumerable<TEntity> entities,
            IDbTransaction transaction = null,
            SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.Default,
            int bulkCopyTimeout = 600,
            int bulkCopyBatchSize = 5000,
            int bulkCopyNotifyAfter = 1000,
            bool bulkCopyEnableStreaming = false,
            bool deleteWhenNotMatched = false,
            bool? outputIdentity = null
            ) where TEntity : class
        {
            if (!entities.Any())
            {
                return;
            }

            var outputIdentityDic = outputIdentity.HasValue && outputIdentity.Value ?
                        new Dictionary<int, TEntity>() : null;

            var dataTable = GetTemporaryDataTable(typeof(TEntity), out var bulkMetadata, outputIdentity).Shred(entities, bulkMetadata, null, outputIdentityDic);

            var sqlConnection = (SqlConnection)connection;
            sqlConnection.Open();

            if (transaction == null)
            {
                transaction = connection.BeginTransaction();
            }

            using (var trans = (SqlTransaction)transaction)
            {
                try
                {
                    var command = sqlConnection.CreateCommand();
                    command.Connection = sqlConnection;
                    command.Transaction = trans;
                    command.CommandTimeout = 600;
                    CheckTemporaryTableQuery(connection, transaction, bulkMetadata, true);
                    //Creating temp table on database
                    command.CommandText = bulkMetadata.TempTableQuery;
                    await command.ExecuteNonQueryAsync();
                    //Bulk copy into temp table
                    await BulkCopyAsync<TEntity>(sqlConnection, trans, dataTable, sqlBulkCopyOptions,
                        bulkMetadata.Name, bulkCopyTimeout, bulkCopyBatchSize, bulkCopyNotifyAfter, bulkCopyEnableStreaming);
                    // Updating destination table, and dropping temp table
                    CheckInsertOrUpdateTableQuery(bulkMetadata);
                    var outputCreateTableQuery = outputIdentity.HasValue && outputIdentity.Value ?
                        $"CREATE TABLE #{bulkMetadata.Name}InsertOutput(InternalId int, Id int); " : "";
                    var outputIdentityQuery = outputIdentity.HasValue && outputIdentity.Value ?
                        $"OUTPUT Source.InternalId, INSERTED.{bulkMetadata.IdentityColumn.Item1} INTO #{bulkMetadata.Name}InsertOutput(InternalId, {bulkMetadata.IdentityColumn.Item1}); " : ";";
                    var deleteWhenNotMatchedQuery = deleteWhenNotMatched ?
                        " WHEN NOT MATCHED BY SOURCE THEN DELETE " : " ";
                    var query = $"{outputCreateTableQuery} {bulkMetadata.UpdateQuery} {deleteWhenNotMatchedQuery} {outputIdentityQuery} DROP TABLE #{bulkMetadata.Name};";

                    command.CommandText = query;
                    await command.ExecuteNonQueryAsync();

                    if (outputIdentity.HasValue && outputIdentity.Value)
                    {
                        command.CommandText = $"SELECT InternalId, Id FROM #{bulkMetadata.Name}InsertOutput;";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (outputIdentityDic.TryGetValue((int)reader[0], out var item))
                                {
                                    var type = item.GetType();
                                    var prop = type.GetProperty(bulkMetadata.IdentityColumn.Item1);
                                    prop.SetValue(item, reader[1], null);
                                }
                            }
                        }
                    }

                    command.CommandText = $"DROP TABLE #{bulkMetadata.Name}InsertOutput;";
                    await command.ExecuteNonQueryAsync();

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    sqlConnection.Close();
                }
            }
        }
    }
}
