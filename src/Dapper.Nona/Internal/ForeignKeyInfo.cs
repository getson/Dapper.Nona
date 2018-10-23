using Dapper.Nona.Abstractions;

namespace Dapper.Nona.Internal
{
    internal class ForeignKeyInfo
    {
        public ForeignKeyInfo(NonaProperty propertyInfo, ForeignKeyRelation relation)
        {
            NonaProperty = propertyInfo;
            Relation = relation;
        }

        public NonaProperty NonaProperty { get; set; }

        public ForeignKeyRelation Relation { get; set; }
    }


}

