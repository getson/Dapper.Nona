using System.Data;
using System.Linq;
using Dapper.Nona.Abstractions;
using Dapper.Nona.FluentMapping;
using Dapper.FluentMap;

namespace Dapper.Nona.Resolvers
{
    /// <inheritdoc />
    /// <summary>
    /// Implements the <see cref="T:Dapper.Nona.Abstractions.IDataColumnResolver" /> interface by using the configured mapping.
    /// </summary>
    public class NonaDataColumnResolver : IDataColumnResolver
    {
        /// <inheritdoc/>
        public DataColumn ResolveDataColumn(NonaProperty propertyInfo)
        {
            if (propertyInfo.Type == null)
                return NonaMapper.Resolvers.Default.DataColumnResolver.ResolveDataColumn(propertyInfo);

            if (!FluentMapper.EntityMaps.TryGetValue(propertyInfo.Type, out var entityMap))
                return NonaMapper.Resolvers.Default.DataColumnResolver.ResolveDataColumn(propertyInfo);

            if (!(entityMap is INonaEntityMap))
                return NonaMapper.Resolvers.Default.DataColumnResolver.ResolveDataColumn(propertyInfo);

            var propertyMaps = entityMap.PropertyMaps.Where(m => m.PropertyInfo.Name == propertyInfo.PropertyInfo.Name).ToList();

            return propertyMaps.Count == 1 ? new DataColumn(propertyMaps[0].ColumnName, propertyMaps[0].PropertyInfo.PropertyType) : 
                NonaMapper.Resolvers.Default.DataColumnResolver.ResolveDataColumn(propertyInfo);
        }
    }
}
