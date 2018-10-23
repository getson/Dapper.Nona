using Dapper.Nona.Abstractions;

namespace Dapper.Nona.Internal
{

    internal class KeyPropertyInfo
    {
        public KeyPropertyInfo(NonaProperty propertyInfo, bool isIdentity)
        {
            NonaProperty = propertyInfo;
            IsIdentity = isIdentity;
        }

        public NonaProperty NonaProperty { get; }

        public bool IsIdentity { get; }
    }

}
