using Dapper.Nona.IntegrationTests.Entities;
using System.Data.SqlClient;
using Dapper.Nona;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dapper.Nona.IntegrationTests
{
    public class BulkInsertTests : BaseTests
    {
        [Fact]
        public void BulkInsertTest()
        {
            var tag = DateTime.Now.Date.ToString("g");
            var entities = new List<Product2>();
            for (var index = 1; index <= 100; index++)
            {
                entities.Add(new Product2
                {
                    ProductName = $"Product {index} {tag}"
                });
            }

            using (var con = new SqlConnection(GetConnectionString()))
            {
                con.Open();
                using (var transaction = con.BeginTransaction())
                {
                    con.BulkInsert(entities, transaction);
                    transaction.Commit();
                }

                var productName = $"Product 100 {tag}";
                var product = con.Select<Product2>(p => p.ProductName == productName).FirstOrDefault();
                Assert.Equal(product?.ProductName, productName);
                con.Close();
            }
        }

        [Fact]
        public void BulkInsertAsyncTest()
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
