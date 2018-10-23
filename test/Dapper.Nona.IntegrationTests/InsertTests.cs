using Dapper.Nona.IntegrationTests.Entities;
using System.Data.SqlClient;
using Dapper.Nona;
using Xunit;

namespace Dapper.Nona.IntegrationTests
{
    public class InsertTests : BaseTests
    {
        [Fact]
        public void InsertDataTest()
        {
            using (var con = new SqlConnection(GetConnectionString()))
            {
                Product2 toBeInserted = new Product2
                {
                    ProductName = "Product Test"
                };
                var insertedId = con.Insert(toBeInserted);
                Assert.NotNull(toBeInserted);
                Assert.True((int)insertedId > 0);

                Assert.Equal(toBeInserted.ProductId, insertedId);
            }
        }

        [Fact]
        public void InsertListTest()
        {
            using (var con = new SqlConnection(GetConnectionString()))
            {
                var insertedProductId = (int)con.Insert(new[]
                {
                    new Product2 {ProductName = "Product Test"},
                    new Product2 {ProductName = "Product Test2"}
                });
                Assert.True(insertedProductId == 2);
            }
        }

    }
}
