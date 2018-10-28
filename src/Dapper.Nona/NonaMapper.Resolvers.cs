using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper.Nona.Abstractions;
using Dapper.Nona.Resolvers;

namespace Dapper.Nona
{
    public static partial class NonaMapper
    {
        private static IPropertyResolver _propertyResolver = new DefaultPropertyResolver();
        private static IKeyPropertyResolver _keyPropertyResolver = new DefaultKeyPropertyResolver();
        private static IForeignKeyPropertyResolver _foreignKeyPropertyResolver = new DefaultForeignKeyPropertyResolver();
        private static ITableNameResolver _tableNameResolver = new DefaultTableNameResolver();
        private static IColumnNameResolver _columnNameResolver = new DefaultColumnNameResolver();
        private static IDataColumnResolver _dataColumnResolver = new DefaultDataColumnResolver();

        /// <summary>
        /// Sets the <see cref="IPropertyResolver"/> implementation for resolving key of entities.
        /// </summary>
        /// <param name="propertyResolver">An instance of <see cref="IPropertyResolver"/>.</param>
        public static void SetPropertyResolver(IPropertyResolver propertyResolver) => _propertyResolver = propertyResolver;

        /// <summary>
        /// Sets the <see cref="IKeyPropertyResolver"/> implementation for resolving key properties of entities.
        /// </summary>
        /// <param name="resolver">An instance of <see cref="IKeyPropertyResolver"/>.</param>
        public static void SetKeyPropertyResolver(IKeyPropertyResolver resolver) => _keyPropertyResolver = resolver;

        /// <summary>
        /// Sets the <see cref="IForeignKeyPropertyResolver"/> implementation for resolving foreign key properties.
        /// </summary>
        /// <param name="resolver">An instance of <see cref="IForeignKeyPropertyResolver"/>.</param>
        public static void SetForeignKeyPropertyResolver(IForeignKeyPropertyResolver resolver) => _foreignKeyPropertyResolver = resolver;

        /// <summary>
        /// Sets the <see cref="ITableNameResolver"/> implementation for resolving table names for entities.
        /// </summary>
        /// <param name="resolver">An instance of <see cref="ITableNameResolver"/>.</param>
        public static void SetTableNameResolver(ITableNameResolver resolver) => _tableNameResolver = resolver;

        /// <summary>
        /// Sets the <see cref="IColumnNameResolver"/> implementation for resolving column names.
        /// </summary>
        /// <param name="resolver">An instance of <see cref="IColumnNameResolver"/>.</param>
        public static void SetColumnNameResolver(IColumnNameResolver resolver) => _columnNameResolver = resolver;

        /// <summary>
        /// Sets the data column resolver.
        /// </summary>
        /// <param name="resolver">The resolver.</param>
        public static void SetDataColumnResolver(IDataColumnResolver resolver) => _dataColumnResolver = resolver;

        /// <summary>
        /// Helper class for retrieving type metadata to build sql queries using configured resolvers.
        /// </summary>
        public static partial class Resolvers
        {
            private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> TypeTableNameCache = new ConcurrentDictionary<RuntimeTypeHandle, string>();
            private static readonly ConcurrentDictionary<string, string> ColumnNameCache = new ConcurrentDictionary<string, string>();
            private static readonly ConcurrentDictionary<string, Tuple<string, Type>> DataColumnCache = new ConcurrentDictionary<string, Tuple<string, Type>>();

            private static readonly ConcurrentDictionary<RuntimeTypeHandle, KeyPropertyInfo> TypeKeyPropertyCache = new ConcurrentDictionary<RuntimeTypeHandle, KeyPropertyInfo>();
            private static readonly ConcurrentDictionary<RuntimeTypeHandle, NonaProperty[]> TypePropertiesCache = new ConcurrentDictionary<RuntimeTypeHandle, NonaProperty[]>();

            private static readonly ConcurrentDictionary<string, ForeignKeyInfo> TypeForeignKeyPropertyCache = new ConcurrentDictionary<string, ForeignKeyInfo>();

            /// <summary>
            /// Gets the key property for the specified type, using the configured <see cref="IKeyPropertyResolver"/>.
            /// </summary>
            /// <param name="type">The <see cref="Type"/> to get the key property for.</param>
            /// <returns>The key property for <paramref name="type"/>.</returns>
            public static NonaProperty KeyProperty(Type type) => KeyProperty(type, out _);

            /// <summary>
            /// Gets the key property for the specified type, using the configured <see cref="IKeyPropertyResolver"/>.
            /// </summary>
            /// <param name="type">The <see cref="Type"/> to get the key property for.</param>
            /// <param name="isIdentity">A value indicating whether the key is an identity.</param>
            /// <returns>The key property for <paramref name="type"/>.</returns>
            public static NonaProperty KeyProperty(Type type, out bool isIdentity)
            {
                if (!TypeKeyPropertyCache.TryGetValue(type.TypeHandle, out var keyProperty))
                {
                    var NonaProperty = _keyPropertyResolver.Resolve(type, out isIdentity);
                    keyProperty = new KeyPropertyInfo(NonaProperty, isIdentity);
                    TypeKeyPropertyCache.TryAdd(type.TypeHandle, keyProperty);
                }

                isIdentity = keyProperty.IsIdentity;

                LogReceived?.Invoke($"Resolved property '{keyProperty.NonaProperty}' (Identity: {isIdentity}) as key property for '{type.Name}'");
                return keyProperty.NonaProperty;
            }

            /// <summary>
            /// Gets the foreign key property for the specified source type and including type
            /// using the configured <see cref="IForeignKeyPropertyResolver"/>.
            /// </summary>
            /// <param name="sourceType">The source type which should contain the foreign key property.</param>
            /// <param name="includingType">The type of the foreign key relation.</param>
            /// <param name="foreignKeyRelation">The foreign key relationship type.</param>
            /// <returns>The foreign key property for <paramref name="sourceType"/> and <paramref name="includingType"/>.</returns>
            public static NonaProperty ForeignKeyProperty(Type sourceType, Type includingType, out ForeignKeyRelation foreignKeyRelation)
            {
                var key = $"{sourceType.FullName};{includingType.FullName}";
                if (!TypeForeignKeyPropertyCache.TryGetValue(key, out var foreignKeyInfo))
                {
                    // Resolve the property and relation.
                    var foreignKeyProperty = _foreignKeyPropertyResolver.Resolve(sourceType, includingType, out foreignKeyRelation);

                    // Cache the info.
                    foreignKeyInfo = new ForeignKeyInfo(foreignKeyProperty, foreignKeyRelation);
                    TypeForeignKeyPropertyCache.TryAdd(key, foreignKeyInfo);
                }

                foreignKeyRelation = foreignKeyInfo.Relation;

                LogReceived?.Invoke($"Resolved property '{foreignKeyInfo.NonaProperty.PropertyInfo}' ({foreignKeyInfo.Relation.ToString()}) as foreign key between '{sourceType.Name}' and '{includingType.Name}'");
                return foreignKeyInfo.NonaProperty;
            }

            /// <summary>
            /// Gets the properties to be mapped for the specified type, using the configured
            /// <see cref="IPropertyResolver"/>.
            /// </summary>
            /// <param name="type">The <see cref="Type"/> to get the properties from.</param>
            /// <returns>>The collection of to be mapped properties of <paramref name="type"/>.</returns>
            public static IEnumerable<NonaProperty> Properties(Type type)
            {
                if (!TypePropertiesCache.TryGetValue(type.TypeHandle, out var properties))
                {
                    properties = _propertyResolver.Resolve(type).ToArray();
                    TypePropertiesCache.TryAdd(type.TypeHandle, properties);
                }

                return properties;
            }

            /// <summary>
            /// Gets the name of the table in the database for the specified type,
            /// using the configured <see cref="ITableNameResolver"/>.
            /// </summary>
            /// <param name="type">The <see cref="Type"/> to get the table name for.</param>
            /// <returns>The table name in the database for <paramref name="type"/>.</returns>
            public static string Table(Type type)
            {
                if (!TypeTableNameCache.TryGetValue(type.TypeHandle, out var name))
                {
                    name = _tableNameResolver.Resolve(type);
                    if (EscapeCharacterStart != char.MinValue || EscapeCharacterEnd != char.MinValue)
                    {
                        name = EscapeCharacterStart + name + EscapeCharacterEnd;
                    }
                    TypeTableNameCache.TryAdd(type.TypeHandle, name);
                }

                LogReceived?.Invoke($"Resolved table name '{name}' for '{type.Name}'");
                return name;
            }

            /// <summary>
            /// Gets the name of the column in the database for the specified type,
            /// using the configured <see cref="IColumnNameResolver"/>.
            /// </summary>
            /// <param name="propertyInfo">The <see cref="NonaProperty"/> to get the column name for.</param>
            /// <returns>The column name in the database for <paramref name="propertyInfo"/>.</returns>
            public static string Column(NonaProperty propertyInfo)
            {
                var key = propertyInfo.PropertyKey;
                if (!ColumnNameCache.TryGetValue(key, out var columnName))
                {
                    columnName = _columnNameResolver.Resolve(propertyInfo);
                    if (EscapeCharacterStart != char.MinValue || EscapeCharacterEnd != char.MinValue)
                    {
                        columnName = EscapeCharacterStart + columnName + EscapeCharacterEnd;
                    }
                    ColumnNameCache.TryAdd(key, columnName);
                }

                LogReceived?.Invoke($"Resolved column name '{columnName}' for '{propertyInfo.PropertyInfo}'");
                return columnName;
            }

            /// <summary>
            /// Gets the the data column for the specified type,
            /// using the configured <see cref="IDataColumnResolver"/>.
            /// </summary>
            /// <param name="propertyInfo">The <see cref="NonaProperty"/> to get the data column for.</param>
            /// <returns>The data column for <paramref name="propertyInfo"/>.</returns>
            public static Tuple<string, Type> DataColumn(NonaProperty propertyInfo)
            {
                var key = propertyInfo.PropertyKey;
                if (!DataColumnCache.TryGetValue(key, out var column))
                {
                    column = _dataColumnResolver.ResolveDataColumn(propertyInfo);
                    DataColumnCache.TryAdd(key, column);
                }

                LogReceived?.Invoke($"Resolved column name '{column.Item1}' for '{propertyInfo.PropertyInfo}'");
                return column;
            }

            private class KeyPropertyInfo
            {
                public KeyPropertyInfo(NonaProperty propertyInfo, bool isIdentity)
                {
                    NonaProperty = propertyInfo;
                    IsIdentity = isIdentity;
                }

                public NonaProperty NonaProperty { get; }

                public bool IsIdentity { get; }
            }

            private class ForeignKeyInfo
            {
                public ForeignKeyInfo(NonaProperty propertyInfo, ForeignKeyRelation relation)
                {
                    NonaProperty = propertyInfo;
                    Relation = relation;
                }

                public NonaProperty NonaProperty { get; }

                public ForeignKeyRelation Relation { get; }
            }

            /// <summary>
            /// Provides access to default resolver implementations.
            /// </summary>
            public static class Default
            {
                /// <summary>
                /// The default column name resolver.
                /// </summary>
                public static readonly IColumnNameResolver ColumnNameResolver = new DefaultColumnNameResolver();

                /// <summary>
                /// The default data column resolver.
                /// </summary>
                public static readonly IDataColumnResolver DataColumnResolver = new DefaultDataColumnResolver();

                /// <summary>
                /// The default property resolver.
                /// </summary>
                public static readonly IPropertyResolver PropertyResolver = new DefaultPropertyResolver();

                /// <summary>
                /// The default key property resolver.
                /// </summary>
                public static readonly IKeyPropertyResolver KeyPropertyResolver = new DefaultKeyPropertyResolver();

                /// <summary>
                /// The default table name resolver.
                /// </summary>
                public static readonly ITableNameResolver TableNameResolver = new DefaultTableNameResolver();
            }
        }
    }
}
