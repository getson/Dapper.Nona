# Nona
Micro ORM that runs on Dapper.
Thanks to https://github.com/henkmollema/Dommel for the initial idea. We started this project since we don't want to use Entity Framework, we had a look on the market and decided to improve Dommel.
We decided to create a new repository because we are thinking a different approach and these repositories will be different.
<hr>
Nona provides a convenient API for CRUD operations using extension methods on the `IDbConnection` interface. The SQL queries are generated based on your POCO entities. Nona also supports LINQ expressions which are being translated to SQL expressions. [Dapper](https://github.com/StackExchange/dapper-dot-net) is used for query execution and object mapping.

Nona also provides extensibility points to change the behavior of resolving table names, column names, the key property and POCO properties. See [Extensibility](https://github.com/getson/Nona#extensibility) for more details.

<hr>
[![Build status](https://ci.appveyor.com/api/projects/status/34ptoeajvubcv95v/branch/master?svg=true)](https://ci.appveyor.com/project/getson/dapper-nona/branch/master)

## Download
[![Download Nona on NuGet](https://imgur.com/Glo1gZx "Download Dapper.Nona on NuGet")](https://www.nuget.org/packages/Dapper.Nona/)

<hr>

## API

#### Retrieving entities by id
```csharp
using (var con = new SqlConnection())
{
   var product = con.Get<Product>(1);
}
```

#### Retrieving all entities in a table
```csharp
using (var con = new SqlConnection())
{
   var products = con.GetAll<Product>().ToList();
}
```

#### Selecting entities using a predicate
Nona allows you to specify a predicate which is being translated into a SQL expression. The arguments in the lambda expression are added as parameters to the command.
```csharp
using (var con = new SqlConnection())
{
   var products = con.Select<Product>(p => p.Name == "Awesome bike");
   
   var products = con.Select<Product>(p => p.Created < new DateTime(2014, 12, 31) && p.InStock > 5);
}
```

#### Inserting entities
```csharp
using (var con = new SqlConnection())
{
   var product = new Product { Name = "Awesome bike", InStock = 4 };
   int id = con.Insert(product);
}
```

#### Updating entities
```csharp
using (var con = new SqlConnection())
{
   var product = con.Get<Product>(1);
   product.LastUpdate = DateTime.Now;
   con.Update(product);
}
```

#### Removing entities
```csharp
using (var con = new SqlConnection())
{
   var product = con.Get<Product>(1);
   con.Delete(product);
}
```

<hr>

## Query builders

Nona supports building specialized queries for a certain RDBMS. By default, query builders for the following RDMBS are included: SQL Server, SQL Server CE, SQLite, MySQL and Postgres. The query builder to be used is determined by the connection type. To add or overwrite an existing query builder, use the `AddSqlBuilder()`  method:

```csharp
NonaMapper.AddSqlBuilder(typeof(SqlConnection), new CustomSqlBuilder());
```

<hr>

## Extensibility
#### `ITableNameResolver`
Implement this interface if you want to customize the resolving of table names when building SQL queries.
```csharp
public class CustomTableNameResolver : ITableNameResolver
{
    public string ResolveTableName(Type type)
    {
        // Every table has prefix 'tbl'.
        return $"tbl{type.Name}";
    }
}
```

Use the `SetTableNameResolver()` method to register the custom implementation:
```csharp
NonaMapper.SetTableNameResolver(new CustomTableNameResolver());
```

#### `IKeyPropertyResolver`
Implement this interface if you want to customize the resolving of the key property of an entity. By default, Nona will search for a property with the `[Key]` attribute, or a column with the name 'Id'.

If you, for example, have the naming convention of `{TypeName}Id` for key properties, you would implement the `IKeyPropertyResolver` like this:
```csharp
public class CustomKeyPropertyResolver : IKeyPropertyResolver
{
    public PropertyInfo ResolveKeyProperty(Type type)
    {
        return type.GetProperties().Single(p => p.Name == $"{type.Name}Id");
    }
}
```

Use the `SetKeyPropertyResolver()` method to register the custom implementation:
```csharp
NonaMapper.SetKeyPropertyResolver(new CustomKeyPropertyResolver());
```

#### `IColumnNameResolver`
Implement this interface if you want to customize the resolving of column names for when building SQL queries. This is useful when your naming conventions for database columns are different than your POCO properties.

```csharp
public class CustomColumnNameResolver : IColumnNameResolver
{
    public string ResolveColumnName(NonaProperty property)
    {
        // Every column has prefix 'fld' and is uppercase.
        return $"fld{property.Name.ToUpper()}";
    }
}
```

Use the `SetColumnNameResolver()` method to register the custom implementation:
```csharp
NonaMapper.SetColumnNameResolver(new CustomColumnNameResolver());

```csharp
public class ProductMap : NonaEntityMap<TEntity>
{
    public ProductMap()
    {
        ToTable("tblProduct");
        
        // ...
    }
}
```

##### `NonaPropertyMap<TEntity>`
This class derives `PropertyMap<TEntity>` and allows you to specify the key property of an entity using the `IsKey` method:

```csharp
public class ProductMap : NonaEntityMap<TEntity>
{
    public ProductMap()
    {
        Map(p => p.Id).IsKey();
    }
}
```
In the `FluentMapper.Initialize()` method you have to call ApplyToNona() in order to tell Nona to use fluent mapping:

```csharp
FluentMapper.Initialize(config =>
    {
        config.AddMap(new ProductMap());
        config.ApplyToNona();
    });
```
