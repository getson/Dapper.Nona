using Dapper.Nona.Internal;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.Nona
{
    public static partial class NonaMapper
    {
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> UpdateQueryCache = new ConcurrentDictionary<RuntimeTypeHandle, string>();

        /// <summary>
        /// Updates the values of the specified entity in the database.
        /// The return value indicates whether the operation succeeded.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="entity">The entity in the database.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <returns>A value indicating whether the update operation succeeded.</returns>
        public static bool Update<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null) where T : class
        {
            var type = TypeHelper.GetConcreteType<T>(out var _);
            var sql = BuildUpdateQuery(type);
            LogQuery<T>(sql);
            return connection.Execute(sql, entity, transaction) > 0;
        }

        /// <summary>
        /// Updates the values of the specified entity in the database.
        /// The return value indicates whether the operation succeeded.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="entity">The entity/list of entity in the database</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <returns>A value indicating whether the update operation succeeded.</returns>
        public static async Task<bool> UpdateAsync<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null) where T : class
        {
            var type = TypeHelper.GetConcreteType<T>(out var _);
            var sql = BuildUpdateQuery(type);
            LogQuery<T>(sql);
            return await connection.ExecuteAsync(sql, entity, transaction) > 0;
        }

        private static string BuildUpdateQuery(Type type)
        {
            if (!UpdateQueryCache.TryGetValue(type.TypeHandle, out var sql))
            {
                var tableName = Resolvers.Table(type);
                var keyProperty = Resolvers.KeyProperty(type);

                // Use all properties which are settable.
                var typeProperties = Resolvers.Properties(type)
                                              .Where(p => p != keyProperty)
                                              .Where(p => p.PropertyInfo.GetSetMethod() != null)
                                              .ToArray();

                var columnNames = typeProperties.Select(p => $"{Resolvers.Column(p)} = @{p.Name}").ToArray();

                sql = $"update {tableName} set {string.Join(", ", columnNames)} where {Resolvers.Column(keyProperty)} = @{keyProperty.Name}";

                UpdateQueryCache.TryAdd(type.TypeHandle, sql);
            }

            return sql;
        }
    }
}
