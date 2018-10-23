using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dapper.Nona
{
    /// <summary>
    /// Simple CRUD operations for Dapper.
    /// </summary>
    public static partial class NonaMapper
    {
        private static readonly ConcurrentDictionary<string, PropertyInfo> ColumnPropertyCache = new ConcurrentDictionary<string, PropertyInfo>();
        /// <summary>
        /// The escape character to use for escaping the start of column and table names in queries.
        /// </summary>
        public static readonly char EscapeCharacterStart;

        /// <summary>
        /// The escape character to use for escaping the end of column and table names in queries.
        /// </summary>
        public static readonly char EscapeCharacterEnd;

        /// <summary>
        /// A callback which gets invoked when queries and other information are logged.
        /// </summary>
        public static Action<string> LogReceived;

        static NonaMapper()
        {
            SqlMapper.TypeMapProvider = type => CreateMap(type);

            SqlMapper.ITypeMap CreateMap(Type t) => new CustomPropertyTypeMap(t,
                (type, columnName) =>
                {
                    var cacheKey = type + columnName;
                    if (!ColumnPropertyCache.TryGetValue(cacheKey, out var propertyInfo))
                    {
                        propertyInfo = type.GetProperties().FirstOrDefault(p => p.GetCustomAttribute<ColumnAttribute>()?.Name == columnName || p.Name == columnName);
                        ColumnPropertyCache.TryAdd(cacheKey, propertyInfo);
                    }

                    return propertyInfo;
                });
        }

        /// <summary>
        /// log query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="method"></param>
        public static void LogQuery<T>(string query, [CallerMemberName]string method = null)
        {
            LogReceived?.Invoke(method != null ? $"{method}<{typeof(T).Name}>: {query}" : query);
        }
    }
}
