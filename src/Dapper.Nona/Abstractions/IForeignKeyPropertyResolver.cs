using System;

namespace Dapper.Nona.Abstractions
{
   
        /// <summary>
        /// Defines methods for resolving foreign key properties.
        /// </summary>
        public interface IForeignKeyPropertyResolver
        {
            /// <summary>
            /// Resolves the foreign key property for the specified source type and including type.
            /// </summary>
            /// <param name="sourceType">The source type which should contain the foreign key property.</param>
            /// <param name="includingType">The type of the foreign key relation.</param>
            /// <param name="foreignKeyRelation">The foreign key relationship type.</param>
            /// <returns>The foreign key property for <paramref name="sourceType"/> and <paramref name="includingType"/>.</returns>
            NonaProperty Resolve(Type sourceType, Type includingType, out ForeignKeyRelation foreignKeyRelation);
        }
    
}
