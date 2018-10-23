using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using Dapper.Nona.Abstractions;

namespace Dapper.Nona.Resolvers
{
    /// <inheritdoc />
    /// <summary>
    /// Implements the <see cref="T:Dapper.Nona.Abstractions.IKeyPropertyResolver" />.
    /// </summary>
    public class DefaultDataColumnResolver : IDataColumnResolver
    {
        /// <inheritdoc />
        /// <summary>
        /// Resolves the column name for the property.
        /// Looks for the [Column] attribute. Otherwise it's just the name of the property.
        /// </summary>
        public virtual DataColumn ResolveDataColumn(NonaProperty propertyInfo)
        {
            var columnAttr = propertyInfo.PropertyInfo.GetCustomAttribute<ColumnAttribute>();
            return columnAttr != null ? new DataColumn(columnAttr.Name, columnAttr.GetType()) : 
                new DataColumn(propertyInfo.PropertyInfo.Name, propertyInfo.PropertyInfo.GetType());
        }
    }
}
