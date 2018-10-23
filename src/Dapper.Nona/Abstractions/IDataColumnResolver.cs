using System.Data;

namespace Dapper.Nona.Abstractions
{
   
        /// <summary>
        /// Defines methods for resolving data column for entities.
        /// Custom implementations can be registerd with <see cref="NonaMapper.SetDataColumnResolver"/>.
        /// </summary>
        public interface IDataColumnResolver
        {

        /// <summary>
        /// Resolves the data column.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns></returns>
        DataColumn ResolveDataColumn(NonaProperty propertyInfo);
        }
    
}
