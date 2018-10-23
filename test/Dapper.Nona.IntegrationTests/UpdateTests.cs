using Dapper.Nona.IntegrationTests.Entities;
using System.Data.SqlClient;
using System.Linq;
using Dapper.Nona;
using Xunit;

namespace Dapper.Nona.IntegrationTests
{
    public class UpdateTests : BaseTests
    {
        [Fact]
        public void UpdateTest()
        {

            using (var con = new SqlConnection(GetConnectionString()))
            {
                var item = new Product2 { ProductName = "product 2 example" };
                var inserted = con.Insert(item);
                item.ProductName = "updated!";

                Assert.True(con.Update(item));
            }
        }
        [Fact]
        public void SelectWithCustomQuery()
        {
            using (var con = new SqlConnection(GetConnectionString()))
            {
                var paged = con.Select<Product2>(p => p.ProductId > 0).ToArray();
                paged = con.Select<Product2>(p => p.ProductId > 0).ToArray();
                paged = con.Select<Product2>(p => p.ProductId > 0).ToArray();
                paged = con.Select<Product2>(p => p.ProductId > 0).ToArray();
                paged = con.Select<Product2>(p => p.ProductId > 0).ToArray();
                Assert.NotEmpty(paged);
            }
        }
    }

}

