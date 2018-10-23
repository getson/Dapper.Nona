using System.ComponentModel.DataAnnotations.Schema;
using Dapper.Nona.Abstractions;
using Dapper.Nona.Resolvers;
using Dapper.Nona;
using Xunit;

namespace Dapper.Nona.Tests
{
    public class DefaultColumnNameResolverTests
    {
        private static readonly DefaultColumnNameResolver Resolver = new DefaultColumnNameResolver();

        [Fact]
        public void ResolvesName()
        {
            var type = typeof(Foo);
            var name = Resolver.Resolve(new NonaProperty(type,type.GetProperty("Bar")));
            Assert.Equal("Bar", name);
        }

        [Fact]
        public void ResolvesColumnAttribute()
        {
            var type = typeof(Bar);
            var name = Resolver.Resolve(new NonaProperty(type,type.GetProperty("FooBarBaz")));
            Assert.Equal("foo_bar_baz", name);
        }

        private class Foo
        {
            public string Bar { get; set; }
        }

        private class Bar
        {
            [Column("foo_bar_baz")]
            public string FooBarBaz { get; set; }
        }
    }
}
