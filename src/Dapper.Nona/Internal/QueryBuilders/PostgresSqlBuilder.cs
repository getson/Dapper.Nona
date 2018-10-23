using System;
using Dapper.Nona.Abstractions;

namespace Dapper.Nona.Internal.QueryBuilders
{

    /// <summary>
    /// <see cref="ISqlBuilder"/> implementation for Postgres.
    /// </summary>
    internal sealed class PostgresSqlBuilder : ISqlBuilder
    {
        /// <inheritdoc/>
        public string BuildInsert(string tableName, string[] columnNames, string[] paramNames, NonaProperty keyProperty)
        {
            var sql = $"insert into {tableName} ({string.Join(", ", columnNames)}) values ({string.Join(", ", paramNames)})";

            if (keyProperty != null)
            {
                var keyColumnName = NonaMapper.Resolvers.Column(keyProperty);

                sql += " RETURNING " + keyColumnName;
            }
            else
            {
                // todo: what behavior is desired here?
                throw new Exception("A key property is required for the PostgresSqlBuilder.");
            }

            return sql;
        }

        public string BuildMultipleInsert(string tableName, string[] columnNames, string[] paramNames)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public string BuildPaging(string orderBy, int pageNumber, int pageSize)
        {
            var start = pageNumber >= 1 ? (pageNumber - 1) * pageSize : 0;
            return $" {orderBy} OFFSET {start} LIMIT {pageSize}";
        }
    }
}

