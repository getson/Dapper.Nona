using Dapper.Nona.Abstractions;
using Dapper.Nona.Resolvers;

namespace Dapper.Nona.Internal
{
    /// <summary>
    /// Provides access to default resolver implementations.
    /// </summary>
    internal class DefaultResolvers
    {
        /// <summary>
        /// The default column name resolver.
        /// </summary>
        internal static readonly IColumnNameResolver ColumnNameResolver = new DefaultColumnNameResolver();

        /// <summary>
        /// The default column name resolver.
        /// </summary>
        internal static readonly IDataColumnResolver DataColumnResolver = new DefaultDataColumnResolver();

        /// <summary>
        /// The default property resolver.
        /// </summary>
        internal static readonly IPropertyResolver PropertyResolver = new DefaultPropertyResolver();

        /// <summary>
        /// The default key property resolver.
        /// </summary>
        internal static readonly IKeyPropertyResolver KeyPropertyResolver = new DefaultKeyPropertyResolver();

        /// <summary>
        /// The default table name resolver.
        /// </summary>
        internal static readonly ITableNameResolver TableNameResolver = new DefaultTableNameResolver();
    }

}
