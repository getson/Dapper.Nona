namespace Dapper.Nona.Abstractions
{

    /// <summary>
    /// Defines methods for building specialized SQL queries.
    /// </summary>
    public interface ISqlBuilder
    {
        /// <summary>
        /// Builds an insert query using the specified table name, column names and parameter names.
        /// A query to fetch the new id will be included as well.
        /// </summary>
        /// <param name="tableName">The name of the table to query.</param>
        /// <param name="columnNames">The names of the columns in the table.</param>
        /// <param name="paramNames">The names of the parameters in the database command.</param>
        /// <param name="keyProperty">
        /// The key property. This can be used to query a specific column for the new id. This is
        /// optional.
        /// </param>
        /// <returns>An insert query including a query to fetch the new id.</returns>
        string BuildInsert(string tableName, string[] columnNames, string[] paramNames, NonaProperty keyProperty);

        /// <summary>
        /// Builds an insert query using the specified table name, column names and parameter names.
        /// </summary>
        /// <param name="tableName">The name of the table to query.</param>
        /// <param name="columnNames">The names of the columns in the table.</param>
        /// <param name="paramNames">The names of the parameters in the database command.</param>
        /// <returns>An insert query including a query to fetch the new id.</returns>
        string BuildMultipleInsert(string tableName, string[] columnNames, string[] paramNames);
        /// <summary>
        /// Builds the paging part to be appended to an existing select query.
        /// </summary>
        /// <param name="orderBy">The order by part of the query.</param>
        /// <param name="pageNumber">The number of the page to fetch, starting at 1.</param>
        /// <param name="pageSize">The page size.</param>
        /// <returns>The paging part of a query.</returns>
        string BuildPaging(string orderBy, int pageNumber, int pageSize);
    }

}
