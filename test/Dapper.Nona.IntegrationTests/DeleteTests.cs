using Dapper.Nona.IntegrationTests.Entities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Dapper.Nona.IntegrationTests
{

    public class DeleteTests : BaseTests
    {

        [Fact]
        public async Task DeleteListAsync()
        {
            var listOfProduct = new List<Product2>
            {
                new Product2
                {
                   ProductName=Guid.NewGuid().ToString()
                },
                new Product2
                {
                    ProductName=Guid.NewGuid().ToString()
                }
            };

            using (var con = new SqlConnection(GetConnectionString()))
            {
                await con.InsertAsync(listOfProduct);
            }
            using (var con = new SqlConnection(GetConnectionString()))
            {
                var products = (await con.SelectAsync<Product2>(x => x.ProductName == listOfProduct[0].ProductName || x.ProductName == listOfProduct[1].ProductName)).ToList();
                Assert.Equal(listOfProduct.Count, products.Count);
            }
        }

    }
}
