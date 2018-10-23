using Dapper.Nona.FluentMapping;
using Dapper.Nona.IntegrationTests.Entities;

namespace Dapper.Nona.IntegrationTests.Map
{
   public class ProductMap: NonaEntityMap<Product2>
    {
        public ProductMap()
        {
            ToTable("Products");
            Map(p => p.ProductId).ToColumn("Id")
                                 .IsKey()
                                 .IsIdentity();
            Map(p => p.ProductName).ToColumn("Name");
        }
    }
}
