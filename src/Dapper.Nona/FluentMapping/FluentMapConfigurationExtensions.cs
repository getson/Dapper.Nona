using Dapper.Nona.Resolvers;
using Dapper.FluentMap.Configuration;

namespace Dapper.Nona.FluentMapping
{
    /// <summary>
    /// Defines methods for configured Dapper.FluentMap.Nona.
    /// </summary>
    public static class FluentMapConfigurationExtensions
    {
        /// <summary>
        /// Configures the specified configuration for Dapper.FluentMap.Nona.
        /// </summary>
        /// <param name="config">The Dapper.FluentMap configuration.</param>
        /// <returns>
        /// The Dapper.FluentMap configuration.
        /// </returns>
        public static FluentMapConfiguration ApplyToNona(this FluentMapConfiguration config)
        {
            NonaMapper.SetColumnNameResolver(new NonaColumnNameResolver());
            NonaMapper.SetDataColumnResolver(new NonaDataColumnResolver());
            NonaMapper.SetKeyPropertyResolver(new NonaKeyPropertyResolver());
            NonaMapper.SetTableNameResolver(new NonaTableNameResolver());
            NonaMapper.SetPropertyResolver(new NonaPropertyResolver());
            return config;
        }
    }
}
