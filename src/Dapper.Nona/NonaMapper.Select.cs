using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dapper.Nona.Internal;

namespace Dapper.Nona
{
    public static partial class NonaMapper
    {
        /// <summary>
        /// Selects all the entities matching the specified predicate.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="predicate">A predicate to filter the results.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="buffered">
        /// A value indicating whether the result of the query should be executed directly,
        /// or when the query is materialized (using <c>ToList()</c> for example).
        /// </param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <returns>
        /// A collection of entities of type <typeparamref name="T"/> matching the specified
        /// <paramref name="predicate"/>.
        /// </returns>
        public static IEnumerable<T> Select<T>(this IDbConnection connection, Expression<Func<T, bool>> predicate, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
        {
            var sql = BuildSelectSql(predicate, out var parameters);
            LogQuery<T>(sql);
            return connection.Query<T>(sql, parameters, transaction, buffered,commandTimeout);
        }

        /// <summary>
        /// Selects all the entities matching the specified predicate.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="predicate">A predicate to filter the results.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <returns>
        /// A collection of entities of type <typeparamref name="T"/> matching the specified
        /// <paramref name="predicate"/>.
        /// </returns>
        public static Task<IEnumerable<T>> SelectAsync<T>(this IDbConnection connection, Expression<Func<T, bool>> predicate, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var sql = BuildSelectSql(predicate, out var parameters);
            LogQuery<T>(sql);
            return connection.QueryAsync<T>(sql, parameters, transaction,commandTimeout);
        }

        /// <summary>
        /// Selects the first entity matching the specified predicate, or a default value if no entity matched.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="predicate">A predicate to filter the results.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <returns>
        /// A instance of type <typeparamref name="T"/> matching the specified
        /// <paramref name="predicate"/>.
        /// </returns>
        public static T FirstOrDefault<T>(this IDbConnection connection, Expression<Func<T, bool>> predicate, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var sql = BuildSelectSql(predicate, out var parameters);
            LogQuery<T>(sql);
            return connection.QueryFirstOrDefault<T>(sql, parameters, transaction,commandTimeout);
        }

        /// <summary>
        /// Selects the first entity matching the specified predicate, or a default value if no entity matched.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="predicate">A predicate to filter the results.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <returns>
        /// A instance of type <typeparamref name="T"/> matching the specified
        /// <paramref name="predicate"/>.
        /// </returns>
        public static Task<T> FirstOrDefaultAsync<T>(this IDbConnection connection, Expression<Func<T, bool>> predicate, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var sql = BuildSelectSql(predicate, out var parameters);
            LogQuery<T>(sql);
            return connection.QueryFirstOrDefaultAsync<T>(sql, parameters, transaction,commandTimeout);
        }

        private static string BuildSelectSql<T>(Expression<Func<T, bool>> predicate, out DynamicParameters parameters)
        {
            var type = typeof(T);
            if (!_getAllQueryCache.TryGetValue(type.TypeHandle, out var sql))
            {
                var tableName = Resolvers.Table(type);
                sql = $"select * from {tableName}";
                _getAllQueryCache.TryAdd(type.TypeHandle, sql);
            }

            sql += new SqlExpression<T>()
                .Where(predicate)
                .ToSql(out parameters);
            return sql;
        }

        /// <summary>
        /// Selects all the entities matching the specified predicate.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="predicate">A predicate to filter the results.</param>
        /// <param name="pageNumber">The number of the page to fetch, starting at 1.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="buffered">
        /// A value indicating whether the result of the query should be executed directly,
        /// or when the query is materialized (using <c>ToList()</c> for example).
        /// </param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <returns>
        /// A collection of entities of type <typeparamref name="T"/> matching the specified
        /// <paramref name="predicate"/>.
        /// </returns>
        public static IEnumerable<T> SelectPaged<T>(this IDbConnection connection, Expression<Func<T, bool>> predicate, int pageNumber, int pageSize, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
        {
            var sql = BuildSelectPagedQuery(connection, predicate, pageNumber, pageSize, out var parameters);
            LogQuery<T>(sql);
            return connection.Query<T>(sql, parameters, transaction, buffered,commandTimeout);
        }

        /// <summary>
        /// Selects all the entities matching the specified predicate.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="predicate">A predicate to filter the results.</param>
        /// <param name="pageNumber">The number of the page to fetch, starting at 1.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <returns>
        /// A collection of entities of type <typeparamref name="T"/> matching the specified
        /// <paramref name="predicate"/>.
        /// </returns>
        public static Task<IEnumerable<T>> SelectPagedAsync<T>(this IDbConnection connection, Expression<Func<T, bool>> predicate, int pageNumber, int pageSize, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var sql = BuildSelectPagedQuery(connection, predicate, pageNumber, pageSize, out var parameters);
            LogQuery<T>(sql);
            return connection.QueryAsync<T>(sql, parameters, transaction,commandTimeout);
        }

        private static string BuildSelectPagedQuery<T>(IDbConnection connection, Expression<Func<T, bool>> predicate, int pageNumber, int pageSize, out DynamicParameters parameters)
        {
            // Start with the select query part.
            var sql = BuildSelectSql(predicate, out parameters);

            // Append  the paging part including the order by.
            var orderBy = "order by " + Resolvers.Column(Resolvers.KeyProperty(typeof(T)));
            sql += GetSqlBuilder(connection).BuildPaging(orderBy, pageNumber, pageSize);
            return sql;
        }
    }
}
