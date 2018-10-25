using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dapper.Nona.Internal
{
    internal class TypeHelper
    {

        public static Type GetConcreteType<T>(out bool isList) where T : class
        {
            var type = typeof(T);
            isList = false;
            //check if type is Array
            if (type.IsArray)
            {
                isList = true;
                return type.GetElementType();
            }
            //check if it is something like IEnumerable
            if (type.IsGenericType())
            {
                var typeInfo = type.GetTypeInfo();
                bool isEnumerable = typeInfo.ImplementedInterfaces
                                            .Any(ti => ti.IsGenericType() && ti.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ||
                                                       typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>);

                if (isEnumerable)
                {
                    isList = true;
                    return type.GetGenericArguments()[0];
                }
            }
            return type;
        }
    }
}
