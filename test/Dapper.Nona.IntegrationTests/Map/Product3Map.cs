using Dapper.Nona.FluentMapping;
using Dapper.Nona.IntegrationTests.Entities;

namespace Dapper.Nona.IntegrationTests.Map
{
    public class LargeProductMap : NonaEntityMap<LargeProduct>
    {
        public LargeProductMap()
        {
            Map(x => x.ProductId).ToColumn("IDARTIKULLI");
            ToTable("T_ARTIKULLI");
        }
    }
}
