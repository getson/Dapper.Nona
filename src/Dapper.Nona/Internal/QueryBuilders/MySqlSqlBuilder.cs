using Dapper.Nona.Abstractions;

namespace Dapper.Nona.Internal.QueryBuilders
{

    /// <summary>
    ///   <see cref="T:Dapper.Nona.ISqlBuilder" /> implementation for MySQL.
    /// </summary>
    /// <seealso cref="ISqlBuilder" />
    /// <inheritdoc />
    public sealed class MySqlSqlBuilder : ISqlBuilder
    {
        /// <summary>
        /// Builds an insert query using the specified table name, column names and parameter names.
        /// A query to fetch the new id will be included as well.
        /// </summary>
        /// <param name="tableName">The name of the table to query.</param>
        /// <param name="columnNames">The names of the columns in the table.</param>
        /// <param name="paramNames">The names of the parameters in the database command.</param>
        /// <param name="keyProperty">The key property. This can be used to query a specific column for the new id. This is
        /// optional.</param>
        /// <returns>
        /// An insert query including a query to fetch the new id.
        /// </returns>
        /// <inheritdoc />
        public string BuildInsert(string tableName, string[] columnNames, string[] paramNames, NonaProperty keyProperty)
        {
            if (NonaMapper.EscapeCharacterStart == char.MinValue && NonaMapper.EscapeCharacterEnd == char.MinValue)
            {
                // Fall back to the default behavior.
                return $"insert into `{tableName}` (`{string.Join("`, `", columnNames)}`) values ({string.Join(", ", paramNames)}); select LAST_INSERT_ID() id";
            }

            // Table and column names are already escaped.
            return $"insert into {tableName} ({string.Join(", ", columnNames)}) values ({string.Join(", ", paramNames)}); select LAST_INSERT_ID() id";
        }

        /// <summary>
        /// Builds the paging part to be appended to an existing select query.
        /// </summary>
        /// <param name="orderBy">The order by part of the query.</param>
        /// <param name="pageNumber">The number of the page to fetch, starting at 1.</param>
        /// <param name="pageSize">The page size.</param>
        /// <returns>
        /// The paging part of a query.
        /// </returns>
        /// <inheritdoc />
        public string BuildPaging(string orderBy, int pageNumber, int pageSize)
        {
            var start = pageNumber >= 1 ? (pageNumber - 1) * pageSize : 0;
            return $" {orderBy} LIMIT {start}, {pageSize}";
        }

        /// <summary>
        /// Builds an insert query using the specified table name, column names and parameter names.
        /// </summary>
        /// <param name="tableName">The name of the table to query.</param>
        /// <param name="columnNames">The names of the columns in the table.</param>
        /// <param name="paramNames">The names of the parameters in the database command.</param>
        /// <returns>
        /// An insert query including a query to fetch the new id.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public string BuildMultipleInsert(string tableName, string[] columnNames, string[] paramNames)
        {
            throw new System.NotImplementedException();
        }
    }
}

