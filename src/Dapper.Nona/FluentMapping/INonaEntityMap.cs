using Dapper.FluentMap.Mapping;

namespace Dapper.Nona.FluentMapping
{
    /// <inheritdoc />
    /// <summary>
    /// Represents a non-typed mapping of an entity for Nona.
    /// </summary>
    public interface INonaEntityMap : IEntityMap
    {
        /// <summary>
        /// Gets the table name for the current entity.
        /// </summary>
        string TableName { get; }
    }
}