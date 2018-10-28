using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dapper.Nona
{
    public static partial class NonaMapper
    {
        /// <summary>
        /// Deletes the specified entities into the database.
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
        public static void BulkDelete<TEntity>(
            this IDbConnection connection,
            IEnumerable<TEntity> entities,
            IDbTransaction transaction = null,
            SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.Default,
            int bulkCopyTimeout = 600,
            int bulkCopyBatchSize = 5000,
            int bulkCopyNotifyAfter = 1000,
            bool bulkCopyEnableStreaming = false
            ) where TEntity : class
        {
            if (!entities.Any()) return;

            var dataTable = GetTemporaryDataTable(typeof(TEntity), out var bulkMetadata, false).Shred(entities, bulkMetadata, null, identifierOnly: true);

            var sqlConnection = (SqlConnection)connection;
            sqlConnection.Open();

            if (transaction == null) transaction = connection.BeginTransaction();

            using (var trans = (SqlTransaction)transaction)
            {
                try
                {
                    var command = sqlConnection.CreateCommand();
                    command.Connection = sqlConnection;
                    command.Transaction = trans;
                    command.CommandTimeout = 600;
                    CheckTemporaryTableQuery(connection, transaction, bulkMetadata, false);
                    //Creating temp table on database
                    command.CommandText = bulkMetadata.TempTableQuery;
                    command.ExecuteNonQuery();
                    //Bulk copy into temp table
                    BulkCopy<TEntity>(sqlConnection, (SqlTransaction)transaction, dataTable, sqlBulkCopyOptions,
                        bulkMetadata.Name, bulkCopyTimeout, bulkCopyBatchSize, bulkCopyNotifyAfter, bulkCopyEnableStreaming);
                    // Updating destination table, and dropping temp table
                    CheckDeleteTableQuery(bulkMetadata);
                    command.CommandText = bulkMetadata.DeleteQuery;
                    command.ExecuteNonQuery();
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

        /// <summary>
        /// Deletes the specified entities into the database asynchronous.
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
        /// <returns></returns>
        public static async Task BulkDeleteAsync<TEntity>(
            this IDbConnection connection,
            IEnumerable<TEntity> entities,
            IDbTransaction transaction = null,
            SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.Default,
            int bulkCopyTimeout = 600,
            int bulkCopyBatchSize = 5000,
            int bulkCopyNotifyAfter = 1000,
            bool bulkCopyEnableStreaming = false
            ) where TEntity : class
        {
            if (!entities.Any()) return;

            var dataTable = GetTemporaryDataTable(typeof(TEntity), out var bulkMetadata, false).Shred(entities, bulkMetadata, null, identifierOnly: true);

            var sqlConnection = (SqlConnection)connection;
            sqlConnection.Open();

            if (transaction == null) transaction = connection.BeginTransaction();

            using (var trans = (SqlTransaction)transaction)
            {
                try
                {
                    var command = sqlConnection.CreateCommand();
                    command.Connection = sqlConnection;
                    command.Transaction = trans;
                    command.CommandTimeout = 600;
                    CheckTemporaryTableQuery(connection, transaction, bulkMetadata, false);
                    //Creating temp table on database
                    command.CommandText = bulkMetadata.TempTableQuery;
                    await command.ExecuteNonQueryAsync();
                    //Bulk copy into temp table
                    await BulkCopyAsync<TEntity>(sqlConnection, (SqlTransaction)transaction, dataTable, sqlBulkCopyOptions,
                        bulkMetadata.Name, bulkCopyTimeout, bulkCopyBatchSize, bulkCopyNotifyAfter, bulkCopyEnableStreaming);
                    // Updating destination table, and dropping temp table
                    CheckDeleteTableQuery(bulkMetadata);
                    command.CommandText = bulkMetadata.DeleteQuery;
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