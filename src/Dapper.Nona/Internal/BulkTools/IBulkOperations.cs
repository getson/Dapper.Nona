using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Dapper.Nona.Internal.BulkTools
{
    internal interface IBulkOperations
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        void CommitTransaction(SqlConnection connection);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        Task CommitTransactionAsync(SqlConnection connection);
    }
}
