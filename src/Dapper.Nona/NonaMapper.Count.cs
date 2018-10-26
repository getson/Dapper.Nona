using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dapper.Nona.Internal;

namespace Dapper.Nona
{
    public static partial class NonaMapper
    {
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> CountQueryCache = new ConcurrentDictionary<RuntimeTypeHandle, string>();

        /// <summary>
        /// Returns the number of entities matching the specified predicate.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="predicate">A predicate to filter the results.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <returns>The number of entities matching the specified predicate.</returns>
        public static long Count<T>(this IDbConnection connection, Expression<Func<T, bool>> predicate, IDbTransaction transaction = null,  int? commandTimeout = null)
        {
            var sql = BuildCountSql(predicate, out var parameters);
            LogQuery<T>(sql);
            return connection.ExecuteScalar<long>(sql, parameters, transaction,commandTimeout);
        }

        /// <summary>
        /// Returns the number of entities matching the specified predicate.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="predicate">A predicate to filter the results.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <returns>The number of entities matching the specified predicate.</returns>
        public static Task<long> CountAsync<T>(this IDbConnection connection, Expression<Func<T, bool>> predicate, IDbTransaction transaction = null,  int? commandTimeout = null)
        {
            var sql = BuildCountSql(predicate, out var parameters);
            LogQuery<T>(sql);
            return connection.ExecuteScalarAsync<long>(sql, parameters, transaction,commandTimeout);
        }

        private static string BuildCountSql<T>(Expression<Func<T, bool>> predicate, out DynamicParameters parameters)
        {
            var type = typeof(T);
            if (!CountQueryCache.TryGetValue(type.TypeHandle, out var sql))
            {
                var tableName = Resolvers.Table(type);
                sql = $"select count(*) from {tableName}";
                CountQueryCache.TryAdd(type.TypeHandle, sql);
            }

            sql += new SqlExpression<T>()
                .Where(predicate)
                .ToSql(out parameters);
            return sql;
        }
    }
}
