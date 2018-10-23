using Dapper.Nona.Abstractions;

namespace Dapper.Nona.Internal.QueryBuilders
{

        /// <summary>
        /// <see cref="ISqlBuilder"/> implementation for SQLite.
        /// </summary>
        internal sealed class SqliteSqlBuilder : ISqlBuilder
        {
            /// <inheritdoc/>
            public string BuildInsert(string tableName, string[] columnNames, string[] paramNames, NonaProperty keyProperty)
            {
                return $"insert into {tableName} ({string.Join(", ", columnNames)}) values ({string.Join(", ", paramNames)}); select last_insert_rowid() id";
            }

            public string BuildMultipleInsert(string tableName, string[] columnNames, string[] paramNames)
            {
                throw new System.NotImplementedException();
            }

            /// <inheritdoc/>
            public string BuildPaging(string orderBy, int pageNumber, int pageSize)
            {
                var start = pageNumber >= 1 ? (pageNumber - 1) * pageSize : 0;
                return $" {orderBy} LIMIT {start}, {pageSize}";
            }
        }
    }

