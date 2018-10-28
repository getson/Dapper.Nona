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
        /// Inserts the specified entities into the database.
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
        public static void BulkInsert<TEntity>(
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
            
            var dataTable = GetTemporaryDataTable(typeof(TEntity), out var bulkMetadata, false).Shred(entities, bulkMetadata, null);

            var sqlConnection = (SqlConnection)connection;
            sqlConnection.Open();

            if (transaction == null) transaction = connection.BeginTransaction();

            using (var bulkCopy = new SqlBulkCopy(sqlConnection, sqlBulkCopyOptions, (SqlTransaction)transaction))
            {
                try
                {
                    bulkCopy.DestinationTableName = bulkMetadata.Name;
                    bulkCopy.SetSettings(bulkCopyEnableStreaming, bulkCopyBatchSize, bulkCopyNotifyAfter, bulkCopyTimeout, rowsCopiedHandler);

                    bulkCopy.WriteToServer(dataTable);

                    transaction.Commit();
                    bulkCopy.Close();
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

            void rowsCopiedHandler(object sender, SqlRowsCopiedEventArgs eventArgs) => LogQuery<TEntity>("Inserted " + eventArgs.RowsCopied + " records.");
        }

        /// <summary>
        /// Inserts the specified entities into the database asynchronous.
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
        /// <returns>
        /// The id of the inserted entity.
        /// </returns>
        public static async Task BulkInsertAsync<TEntity>(
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

            var dataTable = GetTemporaryDataTable(typeof(TEntity), out var bulkMetadata, false).Shred(entities, bulkMetadata, null);

            var sqlConnection = (SqlConnection)connection;
            sqlConnection.Open();

            if (transaction == null) transaction = connection.BeginTransaction();

            using (var bulkCopy = new SqlBulkCopy(sqlConnection, sqlBulkCopyOptions, (SqlTransaction)transaction))
            {
                try
                {
                    bulkCopy.DestinationTableName = bulkMetadata.Name;
                    bulkCopy.SetSettings(bulkCopyEnableStreaming, bulkCopyBatchSize, bulkCopyNotifyAfter, bulkCopyTimeout, rowsCopiedHandler);

                    await bulkCopy.WriteToServerAsync(dataTable);

                    transaction.Commit();
                    bulkCopy.Close();
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

            void rowsCopiedHandler(object sender, SqlRowsCopiedEventArgs eventArgs) => LogQuery<TEntity>("Inserted " + eventArgs.RowsCopied + " records.");
        }
    }
}
