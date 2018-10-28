using System.ComponentModel.DataAnnotations.Schema;
using System;
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
        public virtual Tuple<string, Type> ResolveDataColumn(NonaProperty propertyInfo)
        {
            var columnAttr = propertyInfo.PropertyInfo.GetCustomAttribute<ColumnAttribute>();
            return columnAttr != null ? new Tuple<string, Type>(columnAttr.Name, columnAttr.GetType()) :
                new Tuple<string, Type>(propertyInfo.PropertyInfo.Name, propertyInfo.PropertyInfo.GetType());
        }
    }
}
