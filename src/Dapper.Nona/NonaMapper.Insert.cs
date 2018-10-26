using Dapper.Nona.Abstractions;
using Dapper.Nona.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.Nona
{
    public static partial class NonaMapper
    {
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> InsertQueryCache = new ConcurrentDictionary<RuntimeTypeHandle, string>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> MultiInsertQueryCache = new ConcurrentDictionary<RuntimeTypeHandle, string>();
        /// <summary>
        /// Inserts the specified entity into the database and returns the id.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="entity">The entity/list of entity to be inserted.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <returns>The id of the inserted entity.</returns>
        public static object Insert<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var type = TypeHelper.GetConcreteType<T>(out var isList);

            var sql = BuildInsertQuery(connection, type, isList);
            LogQuery<T>(sql);

            if (isList)
            {
                //return the number of inserted rows
                return connection.Execute(sql, entity, transaction,commandTimeout);
            }
            //get the new id and assign to the key property
            var newId = connection.ExecuteScalar(sql, entity, transaction,commandTimeout);
            var identity = Resolvers.KeyProperty(type);
            if (identity != null)
            {
                identity.PropertyInfo.SetValue(entity, newId);
            }
            //return the newly id
            return newId;
        }
        /// <summary>
        /// Inserts the specified entity into the database and returns the id.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="entity">The entity to be inserted.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <returns>The id of the inserted entity.</returns>
        public static async Task<object> InsertAsync<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null,int? commandTimeout=null) where T : class
        {
            var type = TypeHelper.GetConcreteType<T>(out var isList);
            var sql = BuildInsertQuery(connection, type, isList);
            LogQuery<T>(sql);

            if (isList)
            {
                //return the number of inserted rows
                return await connection.ExecuteAsync(sql, entity, transaction,commandTimeout);
            }
            //get the new id and assign to the key property
            var newId = await connection.ExecuteScalarAsync(sql, entity, transaction,commandTimeout);
            var identity = Resolvers.KeyProperty(type);
            if (identity != null)
            {
                identity.PropertyInfo.SetValue(entity, newId);
            }
            //return the newly id
            return newId;
        }

        private static string BuildInsertQuery(IDbConnection connection, Type type, bool isList)
        {
            if (isList && MultiInsertQueryCache.TryGetValue(type.TypeHandle, out var sql)) return sql;

            if (!isList && InsertQueryCache.TryGetValue(type.TypeHandle, out sql)) return sql;

            var tableName = Resolvers.Table(type);
            var keyProperty = Resolvers.KeyProperty(type, out var isIdentity);

            var typeProperties = new List<NonaProperty>();
            foreach (var typeProperty in Resolvers.Properties(type))
            {
                if (typeProperty == keyProperty)
                {
                    if (isIdentity)
                    {
                        // Skip key properties marked as an identity column.
                        continue;
                    }
                }

                if (typeProperty.PropertyInfo.GetSetMethod() != null)
                {
                    typeProperties.Add(typeProperty);
                }
            }

            var columnNames = typeProperties.Select(Resolvers.Column).ToArray();
            var paramNames = typeProperties.Select(p => "@" + p.Name).ToArray();

            var builder = GetSqlBuilder(connection);
            if (isList)
            {
                sql = builder.BuildMultipleInsert(tableName, columnNames, paramNames);
                MultiInsertQueryCache.TryAdd(type.TypeHandle, sql);
            }
            else
            {
                sql = builder.BuildInsert(tableName, columnNames, paramNames, keyProperty);
                InsertQueryCache.TryAdd(type.TypeHandle, sql);
            }

            return sql;
        }
    }
}
