using System.Linq;
using Dapper.Nona.Abstractions;
using Dapper.Nona.FluentMapping;
using Dapper.Nona.Internal;
using Dapper.FluentMap;

namespace Dapper.Nona.Resolvers
{
    /// <summary>
    /// Implements the <see cref="IColumnNameResolver"/> interface by using the configured mapping.
    /// </summary>
    public class NonaColumnNameResolver : IColumnNameResolver
    {
        /// <inheritdoc/>
        public string Resolve(NonaProperty NonaProperty)
        {
            if (!FluentMapper.EntityMaps.TryGetValue(NonaProperty.Type, out var entityMap))
                return DefaultResolvers.ColumnNameResolver.Resolve(NonaProperty);

            if (!(entityMap is INonaEntityMap))
                return DefaultResolvers.ColumnNameResolver.Resolve(NonaProperty);

            var propertyMaps = entityMap.PropertyMaps.Where(m => m.PropertyInfo.Name == NonaProperty.Name).ToList();

            return propertyMaps.Count == 1 ? propertyMaps[0].ColumnName : DefaultResolvers.ColumnNameResolver.Resolve(NonaProperty);
        }
    }
}
