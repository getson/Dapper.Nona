using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Dapper.Nona.Abstractions;

namespace Dapper.Nona.Resolvers
{
   
        /// <summary>
        /// Implements the <see cref="IForeignKeyPropertyResolver"/> interface.
        /// </summary>
        public class DefaultForeignKeyPropertyResolver : IForeignKeyPropertyResolver
        {
            /// <summary>
            /// Resolves the foreign key property for the specified source type and including type
            /// by using <paramref name="includingType"/> + Id as property name.
            /// </summary>
            /// <param name="sourceType">The source type which should contain the foreign key property.</param>
            /// <param name="includingType">The type of the foreign key relation.</param>
            /// <param name="foreignKeyRelation">The foreign key relationship type.</param>
            /// <returns>The foreign key property for <paramref name="sourceType"/> and <paramref name="includingType"/>.</returns>
            public virtual NonaProperty Resolve(Type sourceType, Type includingType, out ForeignKeyRelation foreignKeyRelation)
            {
                var oneToOneFk = ResolveOneToOne(sourceType, includingType);
                if (oneToOneFk != null)
                {
                    foreignKeyRelation = ForeignKeyRelation.OneToOne;
                    return oneToOneFk;
                }

                var oneToManyFk = ResolveOneToMany(sourceType, includingType);
                if (oneToManyFk != null)
                {
                    foreignKeyRelation = ForeignKeyRelation.OneToMany;
                    return oneToManyFk;
                }

                var msg = $"Could not resolve foreign key property. Source type '{sourceType.FullName}'; including type: '{includingType.FullName}'.";
                throw new Exception(msg);
            }

            private static NonaProperty ResolveOneToOne(Type sourceType, Type includingType)
            {
                // Look for the foreign key on the source type by making an educated guess about the property name.
                var foreignKeyName = includingType.Name + "Id";
                var foreignKeyProperty = sourceType.GetProperty(foreignKeyName);
                if (foreignKeyProperty != null)
                {
                    return new NonaProperty(sourceType, foreignKeyProperty);
                }

                // Determine if the source type contains a navigation property to the including type.
                var navigationProperty = sourceType.GetProperties().FirstOrDefault(p => p.PropertyType == includingType);
                if (navigationProperty != null)
                {
                    // Resolve the foreign key property from the attribute.
                    var fkAttr = navigationProperty.GetCustomAttribute<ForeignKeyAttribute>();
                    if (fkAttr != null)
                    {
                        return new NonaProperty(sourceType, sourceType.GetProperty(fkAttr.Name));
                    }
                }

                return null;
            }

            private static NonaProperty ResolveOneToMany(Type sourceType, Type includingType)
            {
                // Look for the foreign key on the including type by making an educated guess about the property name.
                var foreignKeyName = sourceType.Name + "Id";
                var foreignKeyProperty = includingType.GetProperty(foreignKeyName);
                if (foreignKeyProperty != null)
                {
                    return new NonaProperty(includingType,foreignKeyProperty);
                }

                var collectionType = typeof(IEnumerable<>).MakeGenericType(includingType);
                var navigationProperty = sourceType.GetProperties().FirstOrDefault(p => collectionType.IsAssignableFrom(p.PropertyType));
                if (navigationProperty != null)
                {
                    // Resolve the foreign key property from the attribute.
                    var fkAttr = navigationProperty.GetCustomAttribute<ForeignKeyAttribute>();
                    if (fkAttr != null)
                    {
                        return new NonaProperty(includingType,includingType.GetProperty(fkAttr.Name));
                    }
                }

                return null;
            }
        }
    }

