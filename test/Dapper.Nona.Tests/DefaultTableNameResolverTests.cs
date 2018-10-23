using System;
using System.ComponentModel.DataAnnotations.Schema;
using Dapper.Nona.Resolvers;
using Xunit;

namespace Dapper.Nona.Tests
{
    public class DefaultTableNameResolverTests
    {
        private static readonly DefaultTableNameResolver Resolver = new DefaultTableNameResolver();

        [Theory]
        [InlineData(typeof(Product), "Products")]
        [InlineData(typeof(Products), "Products")]
        [InlineData(typeof(Category), "Categories")]
        public void PluralizesName(Type type, string tableName)
        {
            var name = Resolver.Resolve(type);
            Assert.Equal(tableName, name);
        }

        [Fact]
        public void MapsTableAttribute()
        {
            var name = Resolver.Resolve(typeof(Foo));
            Assert.Equal("tblFoo", name);
        }

        [Fact]
        public void MapsTableAttributeWithSchema()
        {
            var name = Resolver.Resolve(typeof(FooWithSchema));
            Assert.Equal("dbo.tblFoo", name);
        }

        private class Product { }

        private class Products { }

        private class Category { }

        [Table("tblFoo")]
        private class Foo { }

        [Table("tblFoo", Schema = "dbo")]
        private class FooWithSchema { }
    }
}
