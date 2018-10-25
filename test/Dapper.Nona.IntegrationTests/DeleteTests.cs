using Dapper.Nona.IntegrationTests.Entities;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Xunit;

namespace Dapper.Nona.IntegrationTests
{

    public class DeleteTests : BaseTests
    {

        [Fact]
        public void DeleteList()
        {
            var listOfProduct = new List<Product2>
            {
                new Product2
                {
                   ProductName="p1"
                },
                new Product2
                {
                    ProductName="p2"
                }
            };

            using (var con = new SqlConnection(GetConnectionString()))
            {
                var products = con.Select<Product2>(x => x.ProductName == "p1" || x.ProductName == "p2").ToList();
                con.Delete(products);
                con.Insert(listOfProduct);

            }
            using (var con = new SqlConnection(GetConnectionString()))
            {
                var products = con.Select<Product2>(x => x.ProductName == "p1" || x.ProductName == "p2").ToList();
                products[0].ProductName = "p1-updated!";
                products[1].ProductName = "p2-updated!";

                con.Update(products);

                products = con.Select<Product2>(x=>x.ProductId==products[0].ProductId || x.ProductId==products[1].ProductId).ToList();

                Assert.Equal("p1-updated!", products[0].ProductName);
                Assert.Equal("p2-updated!", products[1].ProductName);
            }
        }

    }
}
