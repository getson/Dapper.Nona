using Dapper.Nona.Internal;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dapper.Nona
{
    public static partial class NonaMapper
    {
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> DeleteQueryCache = new ConcurrentDictionary<RuntimeTypeHandle, string>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> DeleteAllQueryCache = new ConcurrentDictionary<RuntimeTypeHandle, string>();

        /// <summary>
        /// Deletes the specified entity from the database.
        /// Returns a value indicating whether the operation succeeded.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="entity">The entity/list of entities to be deleted.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <returns>A value indicating whether the delete operation succeeded.</returns>
        public static bool Delete<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var type = TypeHelper.GetConcreteType<T>(out var _);
            var sql = BuildDeleteQuery(type);
            LogQuery<T>(sql);
            return connection.Execute(sql, entity, transaction,commandTimeout) > 0;
        }
        /// <summary>
        /// Deletes the specified entity from the database.
        /// Returns a value indicating whether the operation succeeded.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="entity">The entity to be deleted.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <returns>A value indicating whether the delete operation succeeded.</returns>
        public static async Task<bool> DeleteAsync<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var type = TypeHelper.GetConcreteType<T>(out var _);
            var sql = BuildDeleteQuery(type);
            LogQuery<T>(sql);
            return await connection.ExecuteAsync(sql, entity, transaction,commandTimeout) > 0;
        }

        private static string BuildDeleteQuery(Type type)
        {
            if (!DeleteQueryCache.TryGetValue(type.TypeHandle, out var sql))
            {
                var tableName = Resolvers.Table(type);
                var keyProperty = Resolvers.KeyProperty(type);
                var keyColumnName = Resolvers.Column(keyProperty);

                sql = $"delete from {tableName} where {keyColumnName} = @{keyProperty.Name}";

                DeleteQueryCache.TryAdd(type.TypeHandle, sql);
            }

            return sql;
        }

        /// <summary>
        /// Deletes all entities of type <typeparamref name="T"/> matching the specified predicate from the database.
        /// Returns a value indicating whether the operation succeeded.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="predicate">A predicate to filter which entities are deleted.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <returns>A value indicating whether the delete operation succeeded.</returns>
        public static bool DeleteMultiple<T>(this IDbConnection connection, Expression<Func<T, bool>> predicate, IDbTransaction transaction = null,  int? commandTimeout = null)
        {
            var sql = BuildDeleteMultipleQuery(predicate, out var parameters);
            LogQuery<T>(sql);
            return connection.Execute(sql, parameters, transaction,commandTimeout) > 0;
        }

        /// <summary>
        /// Deletes all entities of type <typeparamref name="T"/> matching the specified predicate from the database.
        /// Returns a value indicating whether the operation succeeded.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="predicate">A predicate to filter which entities are deleted.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <returns>A value indicating whether the delete operation succeeded.</returns>
        public static async Task<bool> DeleteMultipleAsync<T>(this IDbConnection connection, Expression<Func<T, bool>> predicate, IDbTransaction transaction = null,  int? commandTimeout = null)
        {
            var sql = BuildDeleteMultipleQuery(predicate, out var parameters);
            LogQuery<T>(sql);
            return await connection.ExecuteAsync(sql, parameters, transaction,commandTimeout) > 0;
        }

        private static string BuildDeleteMultipleQuery<T>(Expression<Func<T, bool>> predicate, out DynamicParameters parameters)
        {
            var type = typeof(T);
            if (!DeleteAllQueryCache.TryGetValue(type.TypeHandle, out var sql))
            {
                var tableName = Resolvers.Table(type);
                sql = $"delete from {tableName}";
                DeleteAllQueryCache.TryAdd(type.TypeHandle, sql);
            }

            sql += new SqlExpression<T>()
                .Where(predicate)
                .ToSql(out parameters);
            return sql;
        }

        /// <summary>
        /// Deletes all entities of type <typeparamref name="T"/> from the database.
        /// Returns a value indicating whether the operation succeeded.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <returns>A value indicating whether the delete operation succeeded.</returns>
        public static bool DeleteAll<T>(this IDbConnection connection, IDbTransaction transaction = null,  int? commandTimeout = null)
        {
            var sql = BuildDeleteAllQuery(typeof(T));
            LogQuery<T>(sql);
            return connection.Execute(sql, transaction: transaction,commandTimeout: commandTimeout) > 0;
        }

        /// <summary>
        /// Deletes all entities of type <typeparamref name="T"/> from the database.
        /// Returns a value indicating whether the operation succeeded.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <returns>A value indicating whether the delete operation succeeded.</returns>
        public static async Task<bool> DeleteAllAsync<T>(this IDbConnection connection, IDbTransaction transaction = null,  int? commandTimeout = null)
        {
            var sql = BuildDeleteAllQuery(typeof(T));
            LogQuery<T>(sql);
            return await connection.ExecuteAsync(sql, transaction: transaction,commandTimeout: commandTimeout) > 0;
        }

        private static string BuildDeleteAllQuery(Type type)
        {
            if (!DeleteAllQueryCache.TryGetValue(type.TypeHandle, out var sql))
            {
                var tableName = Resolvers.Table(type);
                sql = $"delete from {tableName}";
                DeleteAllQueryCache.TryAdd(type.TypeHandle, sql);
            }

            return sql;
        }
    }
}
