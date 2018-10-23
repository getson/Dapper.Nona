using System;
using System.Collections.Generic;
using System.Reflection;
using Dapper.Nona.Abstractions;

namespace Dapper.Nona.Resolvers
{
   
        /// <summary>
        /// Default implemenation of the <see cref="IPropertyResolver"/> interface.
        /// </summary>
        public class DefaultPropertyResolver : IPropertyResolver
        {
            private static readonly HashSet<Type> _primitiveTypes = new HashSet<Type>
            {
                typeof(object),
                typeof(string),
                typeof(Guid),
                typeof(decimal),
                typeof(double),
                typeof(float),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(byte[])
            };

            /// <summary>
            /// Resolves the properties to be mapped for the specified type.
            /// </summary>
            /// <param name="type">The type to resolve the properties to be mapped for.</param>
            /// <returns>A collection of <see cref="NonaProperty"/>'s of the <paramref name="type"/>.</returns>
            public virtual IEnumerable<NonaProperty> Resolve(Type type) => FilterComplexTypes(type);

            /// <summary>
            /// Gets a collection of types that are considered 'primitive' for Nona but are not for the CLR.
            /// Override this to specify your own set of types.
            /// </summary>
            protected virtual HashSet<Type> PrimitiveTypes => _primitiveTypes;

            /// <summary>
            /// Filters the complex types from the specified collection of properties.
            /// </summary>
            /// <returns>The properties that are considered 'primitive' of <paramref />.</returns>
            protected virtual IEnumerable<NonaProperty> FilterComplexTypes(Type type)
            {
                foreach (var property in type.GetProperties())
                {
                    var propertyType = property.PropertyType;
                    propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

                    if (propertyType.GetTypeInfo().IsPrimitive || propertyType.GetTypeInfo().IsEnum || PrimitiveTypes.Contains(propertyType))
                    {
                        yield return new NonaProperty(type,property);
                    }
                }
            }
        }
    }

