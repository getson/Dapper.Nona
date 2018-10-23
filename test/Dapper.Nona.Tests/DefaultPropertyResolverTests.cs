using System;
using System.Collections.Generic;
using System.Linq;
using Dapper.Nona.Resolvers;
using Xunit;

namespace Dapper.Nona.Tests
{
    public class DefaultPropertyResolverTests
    {
        [Fact]
        public void ResolvesSimpleProperties()
        {
            var resolver = new DefaultPropertyResolver();
            var type = typeof(Foo);
            var props = resolver.Resolve(type).Select(x=>x.PropertyInfo).ToArray();
            Assert.Equal(type.GetProperties().Skip(1).ToArray(), props);
        }

        [Fact]
        public void Resolves_WithCustom()
        {
            var resolver = new CustomResolver();
            var type = typeof(Foo);
            var props = resolver.Resolve(type).Select(x => x.PropertyInfo).ToArray();
            Assert.Equal(type.GetProperties().Skip(2).ToArray(), props);
        }

        private class CustomResolver : DefaultPropertyResolver
        {
            // Create a new hashset without the object type.
            protected override HashSet<Type> PrimitiveTypes => new HashSet<Type>(base.PrimitiveTypes.Skip(1));
        }

        private class Foo
        {
            public class Bar { }

            public Bar Baz { get; set; }

            public object Object { get; set; }

            public string String { get; set; }

            public Guid? Guid { get; set; }

            public decimal? Decimal { get; set; }

            public double? Double { get; set; }

            public float? Float { get; set; }

            public DateTime DateTime { get; set; }

            public DateTimeOffset? DateTimeOffset { get; set; }

            public TimeSpan? Timespan { get; set; }

            public byte[] Bytes { get; set; }
        }

        /*
            typeof(object),
            typeof(string),
            typeof(Guid),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(byte[])
         */
    }
}
