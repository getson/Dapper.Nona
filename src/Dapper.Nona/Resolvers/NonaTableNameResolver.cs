using System;
using Dapper.Nona.Abstractions;
using Dapper.Nona.FluentMapping;
using Dapper.Nona.Internal;
using Dapper.FluentMap;

namespace Dapper.Nona.Resolvers
{
    /// <inheritdoc />
    /// <summary>
    /// Implements the <see cref="T:Dapper.Nona.Abstractions.ITableNameResolver" /> interface by using the configured mapping.
    /// </summary>
    public class NonaTableNameResolver : ITableNameResolver
    {
        /// <summary>
        /// Resolves the table name for the specified type.
        /// </summary>
        /// <param name="type">The type to resolve the table name for.</param>
        /// <returns>
        /// A string containing the resolved table name for for <paramref name="type" />.
        /// </returns>
        /// <inheritdoc />
        public string Resolve(Type type)
        {
            if (!FluentMapper.EntityMaps.TryGetValue(type, out var entityMap))
                return NonaMapper.Resolvers.Default.TableNameResolver.Resolve(type);
            if (entityMap is INonaEntityMap mapping)
            {
                return mapping.TableName;
            }

            return DefaultResolvers.TableNameResolver.Resolve(type);
        }
    }
}
