using System.Reflection;
using Dapper.FluentMap.Mapping;

namespace Dapper.Nona.FluentMapping
{
    /// <summary>
    /// Represents the typed mapping of an entity for Nona.
    /// </summary>
    /// <typeparam name="TEntity">The type of an entity.</typeparam>
    /// <seealso>
    ///     <cref>Dapper.FluentMap.Mapping.EntityMapBase{TEntity, Dapper.Nona.FluentMapping.NonaPropertyMap}</cref>
    /// </seealso>
    /// <seealso cref="INonaEntityMap" />
    /// <inheritdoc>
    ///     <cref>Dapper.FluentMap.Mapping.EntityMapBase{TEntity, Dapper.Nona.FluentMapping.NonaPropertyMap}</cref>
    ///     <cref>Dapper.Nona.FluentMapping.INonaEntityMap</cref>
    /// </inheritdoc>
    public abstract class NonaEntityMap<TEntity> : EntityMapBase<TEntity, NonaPropertyMap>, INonaEntityMap
        where TEntity : class
    {
        /// <inheritdoc />
        /// <summary>
        /// Gets the <see cref="T:Dapper.FluentMap.Mapping.IPropertyMap" /> implementation for the current entity map.
        /// </summary>
        /// <param name="info">The information about the property.</param>
        /// <returns>
        /// An implementation of <see cref="T:Dapper.FluentMap.Mapping.IPropertyMap" />.
        /// </returns>
        protected override NonaPropertyMap GetPropertyMap(PropertyInfo info)
        {
            return new NonaPropertyMap(info);
        }

        /// <summary>
        /// Gets the table name for this entity map.
        /// </summary>
        /// <inheritdoc />
        public string TableName { get; private set; }

        /// <summary>
        /// Sets the table name for the current entity.
        /// </summary>
        /// <param name="tableName">The name of the table in the database.</param>
        protected void ToTable(string tableName)
        {
            TableName = tableName;
        }

        /// <summary>
        /// Sets the table name for the current entity.
        /// </summary>
        /// <param name="tableName">The name of the table in the database.</param>
        /// <param name="schemaName">The name of the table schema in the database.</param>
        protected void ToTable(string tableName, string schemaName)
        {
            TableName = $"{schemaName}.{tableName}";
        }
    }
}
