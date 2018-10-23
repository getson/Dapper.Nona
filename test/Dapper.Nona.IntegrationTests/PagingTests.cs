using Dapper.Nona.IntegrationTests.Entities;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper.Nona;
using Xunit;

namespace Dapper.Nona.IntegrationTests
{
    public class PagingTests : BaseTests
    {
        private readonly List<Product> _products = new List<Product>
        {
            new Product{Name="Chai"},
            new Product{Name="Chang"},
            new Product{Name="Aniseed Syrup"},
            new Product{Name="Chef Anton's Cajun Seasoning"},
            new Product{Name="Chef Anton's Gumbo Mix"},
            new Product{Name="aaaa"},
            new Product{Name="Chand d d     g"},
            new Product{Name="Aniseed Syrup a"},
            new Product{Name="Chef Anton's Caddd jun Seasoning"},
            new Product{Name="Chef Anton's Gumbo Mix To Mix"}

        };

        public PagingTests()
        {
            DeleteAll();
            using (var con = new SqlConnection(GetConnectionString()))
            {
               var inserted= con.Insert(_products);
            }
        }
        [Fact]
        public void Fetches_FirstPage()
        {
            DeleteAll();
            using (var con = new SqlConnection(GetConnectionString()))
            {
                con.Insert(_products);

                var paged = con.GetPaged<Product>(1, 5).ToArray();
                Assert.Equal(5, paged.Length);
                Assert.Collection(paged,
                    p => Assert.Equal("Chai", p.Name),
                    p => Assert.Equal("Chang", p.Name),
                    p => Assert.Equal("Aniseed Syrup", p.Name),
                    p => Assert.Equal("Chef Anton's Cajun Seasoning", p.Name),
                    p => Assert.Equal("Chef Anton's Gumbo Mix", p.Name));
            }
        }

        [Fact]
        public void Fetches_SecondPage()
        {
            using (var con = new SqlConnection(GetConnectionString()))
            {
                var paged = con.GetPaged<Product>(2, 5).ToArray();
                Assert.Equal(5, paged.Length);
            }
        }
        [Fact]
        public async Task SelectPaged_FetchesFirstPageAsync()
        {
            using (var con = new SqlConnection(GetConnectionString()))
            {
                var paged = (await con.SelectPagedAsync<Product>(p => p.Name == "Chai", 1, 5)).ToArray();
                Assert.Single(paged);
            }
        }
    }
}
