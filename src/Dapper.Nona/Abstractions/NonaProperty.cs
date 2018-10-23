using System;
using System.Reflection;

namespace Dapper.Nona.Abstractions
{
    /// <summary>
    /// Represent a Nona Property, it contains the propertyInfo and the specified type
    /// </summary>
    public sealed class NonaProperty : IEquatable<NonaProperty>
    {
        private string _propertyKey;

        /// <inheritdoc />
        public override int GetHashCode()
        {
           return PropertyKey.GetHashCode();
        }
        /// <summary>
        /// Create a  Nona Property with type and parameter 
        /// </summary>
        /// <param name="type">the specified type we want to map</param>
        /// <param name="propertyInfo">the specific property</param>
        public NonaProperty(Type type, PropertyInfo propertyInfo)
        {
          
            Type = type;
            PropertyInfo = propertyInfo;
        }
        /// <summary>
        /// the type that owns this property
        /// </summary>
        public Type Type { get; }
        /// <summary>
        /// property info for this property
        /// </summary>
        public PropertyInfo PropertyInfo { get; }
        /// <summary>
        /// Property Name
        /// </summary>
        public string Name => PropertyInfo.Name;
        /// <summary>
        /// composed key for this property {Type}{PropertyName}
        /// </summary>
        public string PropertyKey
        {
            get
            {
                if (!string.IsNullOrEmpty(_propertyKey))
                    return _propertyKey;

                _propertyKey = $"{Type}.{PropertyInfo.Name}";
                return _propertyKey;
            }
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {

            if (obj is NonaProperty property)
            {
                return EqualsCore(property);
            }
            return false;
        }
        /// <summary>
        /// check if property keys are equal
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EqualsCore(NonaProperty other)
        {
          
            return PropertyKey == other.PropertyKey;
        }

        /// <inheritdoc />
        public bool Equals(NonaProperty other)
        {
            return EqualsCore(other);
        }
        /// <summary>
        /// overload ==
        /// </summary>
        /// <param name="property1"></param>
        /// <param name="property2"></param>
        /// <returns></returns>
        public static bool operator ==(NonaProperty property1, NonaProperty property2)
        {
            if (ReferenceEquals(property1, null) && ReferenceEquals(property2, null))
            {
                return true;
            }

            if (ReferenceEquals(property1, null) || ReferenceEquals(property2, null))
            {
                return false;
            }
            return property1.EqualsCore(property2);
        }
        /// <summary>
        /// overload !==
        /// </summary>
        /// <param name="property1"></param>
        /// <param name="property2"></param>
        /// <returns></returns>
        public static bool operator !=(NonaProperty property1, NonaProperty property2)
        {
           return !(property1 == property2);
        }
    }
}
