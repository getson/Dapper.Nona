using System;
using System.Linq;
using Dapper.Nona.Abstractions;
using Dapper.Nona.FluentMapping;
using Dapper.Nona.Internal;
using Dapper.FluentMap;
using Dapper.FluentMap.Mapping;

namespace Dapper.Nona.Resolvers
{
    /// <summary>
    /// Implements the <see cref="IKeyPropertyResolver"/> interface by using the configured mapping.
    /// </summary>
    public class NonaKeyPropertyResolver : IKeyPropertyResolver
    {
        /// <inheritdoc/>
        public NonaProperty Resolve(Type type)
        {
            bool isIdentity;
            return Resolve(type, out isIdentity);
        }

        /// <inheritdoc/>
        public NonaProperty Resolve(Type type, out bool isIdentity)
        {
            IEntityMap entityMap;
            if (!FluentMapper.EntityMaps.TryGetValue(type, out entityMap))
            {
                return DefaultResolvers.KeyPropertyResolver.Resolve(type, out isIdentity);
            }

            if (entityMap is INonaEntityMap mapping)
            {
                var keyPropertyMaps = entityMap.PropertyMaps.OfType<NonaPropertyMap>().Where(e => e.Key).ToList();

                if (keyPropertyMaps.Count == 1)
                {
                    var keyPropertyMap = keyPropertyMaps[0];
                    isIdentity = keyPropertyMap.Identity;
                    return new NonaProperty(type, keyPropertyMap.PropertyInfo);
                }

                if (keyPropertyMaps.Count > 1)
                {
                    var msg = $"Found multiple key properties on type '{type.FullName}'. This is not yet supported. The following key properties were found:{Environment.NewLine}{string.Join(Environment.NewLine, keyPropertyMaps.Select(t => t.PropertyInfo.Name))}";

                    throw new Exception(msg);
                }
            }

            // Fall back to the default mapping strategy.
            return DefaultResolvers.KeyPropertyResolver.Resolve(type, out isIdentity);
        }
    }
}
