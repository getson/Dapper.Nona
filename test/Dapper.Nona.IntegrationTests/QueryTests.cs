using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Dapper.Nona;
using Dapper.Nona.IntegrationTests.Entities;
using Dapper.Nona.IntegrationTests.Map;
using Dapper.FluentMap;
using Xunit;

namespace Dapper.Nona.IntegrationTests
{
    public class QueryTests:BaseTests
    {
        [Fact]
        public void SelectWithCustomQuery()
        {
            using (var con = new SqlConnection(GetConnectionString()))
            {
                var paged = con.Select<Product2>(p => p.ProductId >0).ToArray();
                 paged = con.Select<Product2>(p => p.ProductId > 0).ToArray();
                 paged = con.Select<Product2>(p => p.ProductId > 0).ToArray();
                 paged = con.Select<Product2>(p => p.ProductId > 0).ToArray();
                 paged = con.Select<Product2>(p => p.ProductId > 0).ToArray();
                Assert.NotEmpty(paged);
            }
        }

        [Fact]
        public void SelectLargeEntity()
        {
            using (var con = new SqlConnection(GetConnectionString()))
            {

                var paged = con.Select<LargeProduct>(p => p.ProductId > 0).ToArray();
                paged = con.Select<LargeProduct>(p => p.ProductId > 0 && p.Idndermarje== 728).ToArray();
                paged = con.Select<LargeProduct>(p => p.ProductId > 0 &&p.Idndermarje==728 &&  p.Pershkrimartikulli == "Biskota").ToArray();
                paged = con.Select<LargeProduct>(p => p.ProductId > 0 && p.Idndermarje == 728 && (p.Pershkrimartikulli == "Biskota" || p.Pershkrimartikulli== "BOLT")).ToArray();
                paged = con.Select<LargeProduct>(p => p.ProductId > 0).ToArray();
                paged = con.Select<LargeProduct>(p => p.ProductId > 0).ToArray();
                Assert.NotEmpty(paged);
            }
        }
        [Fact]
        public void ComplextPredicateTest()
        {
            using (var con = new SqlConnection(GetConnectionString()))
            {

                var result = con.Select<LargeProduct>(p => p.ProductId > 0 &&
                                                           p.Idndermarje == 728 &&
                                                           p.Kodartikulli != "" && p.Kodartikulli != null &&
                                                           (p.Pershkrimartikulli == "Biskota" || p.Pershkrimartikulli == "BOLT") &&
                                                           (p.Idperdoruesi == 3655 || p.Idperdoruesi == 3824 || p.Idperdoruesi == 3666),
                                                     buffered: false
                                                     );
                Assert.True(result.ToArray().Length == 3);
       
            }
        }
    }

}

