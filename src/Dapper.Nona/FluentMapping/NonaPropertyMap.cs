using System.Reflection;
using Dapper.FluentMap.Mapping;

namespace Dapper.Nona.FluentMapping
{
    /// <summary>
    /// Represents mapping of a property for Nona.
    /// </summary>
    /// <seealso>
    ///     <cref>Dapper.FluentMap.Mapping.PropertyMapBase{Dapper.Nona.FluentMapping.NonaPropertyMap}</cref>
    /// </seealso>
    /// <seealso cref="Dapper.FluentMap.Mapping.IPropertyMap" />
    /// <inheritdoc>
    ///     <cref>Dapper.FluentMap.Mapping.PropertyMapBase{Dapper.Nona.FluentMapping.NonaPropertyMap}</cref>
    ///     <cref>Dapper.FluentMap.Mapping.IPropertyMap</cref>
    /// </inheritdoc>
    public class NonaPropertyMap : PropertyMapBase<NonaPropertyMap>, IPropertyMap
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Dapper.Nona.FluentMapping.NonaPropertyMap" /> class
        /// with the specified <see cref="T:System.Reflection.PropertyInfo" /> object.
        /// </summary>
        /// <param name="info">The information about the property.</param>
        public NonaPropertyMap(PropertyInfo info) : base(info)
        {
        }

        /// <summary>
        /// Gets a value indicating whether this property is a primary key.
        /// </summary>
        /// <value>
        ///   <c>true</c> if key; otherwise, <c>false</c>.
        /// </value>
        public bool Key { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this primary key is an identity.
        /// </summary>
        /// <value>
        ///   <c>true</c> if identity; otherwise, <c>false</c>.
        /// </value>
        public bool Identity { get; set; }

        /// <summary>
        /// Specifies the current property as key for the entity.
        /// </summary>
        /// <returns>
        /// The current instance of <see cref="NonaPropertyMap" />.
        /// </returns>
        public NonaPropertyMap IsKey()
        {
            Key = true;
            return this;
        }

        /// <summary>
        /// Specifies the current property as an identity.
        /// </summary>
        /// <returns>
        /// The current instance of <see cref="NonaPropertyMap" />.
        /// </returns>
        public NonaPropertyMap IsIdentity()
        {
            Identity = true;
            return this;
        }
    }
}
