using Dapper.Nona.Abstractions;

namespace Dapper.Nona.Internal.QueryBuilders
{

    /// <summary>
    /// <see cref="ISqlBuilder"/> implementation for SQL Server.
    /// </summary>
    internal sealed class SqlServerSqlBuilder : ISqlBuilder
    {
        /// <inheritdoc/>
        public string BuildInsert(string tableName, string[] columnNames, string[] paramNames, NonaProperty keyProperty)
        {
            return $"set nocount on insert into {tableName} ({string.Join(", ", columnNames)}) values ({string.Join(", ", paramNames)}) select cast(scope_identity() as int)";
        }
        /// <inheritdoc/>
        public string BuildMultipleInsert(string tableName, string[] columnNames, string[] paramNames)
        {
            return $"insert into {tableName} ({string.Join(", ", columnNames)}) values ({string.Join(", ", paramNames)})";
        }

        /// <inheritdoc/>
        public string BuildPaging(string orderBy, int pageNumber, int pageSize)
        {
            var start = pageNumber >= 1 ? (pageNumber - 1) * pageSize : 0;
            return $" {orderBy} offset {start} rows fetch next {pageSize} rows only";
        }
    }
}