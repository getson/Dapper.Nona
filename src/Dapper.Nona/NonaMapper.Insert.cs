using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dapper.Nona.Abstractions;
using Dapper.Nona.Internal;

namespace Dapper.Nona
{
    public static partial class NonaMapper
    {
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> InsertQueryCache = new ConcurrentDictionary<RuntimeTypeHandle, string>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> MultiInsertQueryCache = new ConcurrentDictionary<RuntimeTypeHandle, string>();
        /// <summary>
        /// Inserts the specified entity into the database and returns the id.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="entity">The entity/list of entity to be inserted.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <returns>The id of the inserted entity.</returns>
        public static object Insert<TEntity>(this IDbConnection connection, TEntity entity, IDbTransaction transaction = null) where TEntity : class
        {
            CheckIfTypeIsList<TEntity>(out var type, out var isList);

            var sql = BuildInsertQuery(connection, type, isList);
            LogQuery<TEntity>(sql);

            if (isList)
            {
                //return the number of inserted rows
                return connection.Execute(sql, entity, transaction);
            }
            //get the new id and assign to the key property
            var newId = connection.ExecuteScalar(sql, entity, transaction);
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
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="entity">The entity to be inserted.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <returns>The id of the inserted entity.</returns>
        public static async Task<object> InsertAsync<TEntity>(this IDbConnection connection, TEntity entity, IDbTransaction transaction = null) where TEntity : class
        {
            CheckIfTypeIsList<TEntity>(out var type, out var isList);
            var sql = BuildInsertQuery(connection, type, isList);
            LogQuery<TEntity>(sql);

            if (isList)
            {
                //return the number of inserted rows
                return await connection.ExecuteAsync(sql, entity, transaction);
            }
            //get the new id and assign to the key property
            var newId = await connection.ExecuteScalarAsync(sql, entity, transaction);
            var identity = Resolvers.KeyProperty(type);
            if (identity != null)
            {
                identity.PropertyInfo.SetValue(entity, newId);
            }
            //return the newly id
            return newId;
        }
        private static void CheckIfTypeIsList<TEntity>(out Type type, out bool isList) where TEntity : class
        {
            type = typeof(TEntity);
            isList = false;
            if (type.IsArray)
            {
                isList = true;
                type = type.GetElementType();
            }
            else if (type.IsGenericType())
            {
                var typeInfo = type.GetTypeInfo();
                bool isEnumerable = typeInfo.ImplementedInterfaces
                                        .Any(ti => ti.IsGenericType() &&
                                                   ti.GetGenericTypeDefinition() == typeof(IEnumerable<>)) || typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>);

                if (isEnumerable)
                {
                    isList = true;
                    type = type.GetGenericArguments()[0];
                }
            }
        }
        private static string BuildInsertQuery(IDbConnection connection, Type type, bool isList)
        {
            if (isList && MultiInsertQueryCache.TryGetValue(type.TypeHandle, out var sql)) return sql;

            if (InsertQueryCache.TryGetValue(type.TypeHandle, out sql)) return sql;

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
