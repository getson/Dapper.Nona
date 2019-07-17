using Dapper.Nona.IntegrationTests.Entities;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Dapper.Nona.IntegrationTests
{
    public class UpdateTests : BaseTests
    {
        [Fact]
        public async Task UpdateTest()
        {

            using (var con = new SqlConnection(GetConnectionString()))
            {
                var item = new Product2
                {
                    ProductName = "product 2 example"
                };

                await con.InsertAsync(item);

                item.ProductName = Guid.NewGuid().ToString();

                await con.UpdateAsync(item);

                var updated = (await con.SelectAsync<Product2>(x => x.ProductName == item.ProductName)).FirstOrDefault();

                Assert.NotNull(updated);
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

