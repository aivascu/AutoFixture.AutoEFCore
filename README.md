# EntityFrameworkCore.AutoFixture

![GitHub Workflow Status](https://img.shields.io/github/workflow/status/aivascu/EntityFrameworkCore.AutoFixture/Release%20CD?logo=github&style=flat-square)
[![Coveralls github](https://img.shields.io/coveralls/github/aivascu/EntityFrameworkCore.AutoFixture?logo=coveralls&style=flat-square)](https://coveralls.io/github/aivascu/EntityFrameworkCore.AutoFixture?branch=master)
[![Total alerts](https://img.shields.io/lgtm/alerts/g/aivascu/EntityFrameworkCore.AutoFixture.svg?logo=lgtm&logoWidth=18&style=flat-square)](https://lgtm.com/projects/g/aivascu/EntityFrameworkCore.AutoFixture/alerts/)
[![Nuget](https://img.shields.io/nuget/v/EntityFrameworkCore.AutoFixture?logo=nuget&style=flat-square)](https://www.nuget.org/packages/EntityFrameworkCore.AutoFixture/)
[![GitHub](https://img.shields.io/github/license/aivascu/EntityFrameworkCore.AutoFixture?logo=MIT&style=flat-square)](https://licenses.nuget.org/MIT)

**EntityFrameworkCore.AutoFixture** is the logical product
of [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) in-memory providers
and [AutoFixture](https://github.com/AutoFixture/AutoFixture).

Using **EntityFrameworkCore.AutoFixture** you can greatly reduce the boilerplate work necessary to unit test code that
uses **Entity Framework Core** database contexts (see [examples](#examples)). You'll appreciate this library if you are
already using **AutoFixture** as your auto-mocking container.

**EntityFrameworkCore.AutoFixture** extens **AutoFixture** with the ability to create fully functional `DbContext`
instances, with very little setup code.

Unlike other libraries for faking EF contexts, **EntityFrameworkCore.AutoFixture** does not use mocking frameworks or
dynamic proxies in order to create `DbContext` instances, instead it uses the Microsoft's own
in-memory [providers](https://docs.microsoft.com/en-us/ef/core/miscellaneous/testing/) for EF Core. This allows to make
less assumptions (read as: mock setups) in your tests about how the `DbContext` will behave in the real environment.

#### :warning: .NET Standard 2.0 in EF Core v3.0.x :warning:

Entity Framework Core `v3.0.0` - `v3.0.3` are targeting `netstandard2.1`, which means they are not compatible with
target frameworks that support at most `netstandard2.0` (`>= net47` and `netcoreapp2.1`).
Versions after `v3.1` are targeting `netstandard2.0`. If you've encountered this issue consider upgrading to a later
version of Entity Framework Core.

## Features

**EntityFrameworkCore.AutoFixture** offers three customizations to aid your unit testing workflow:

- `InMemoryContextCustomization` - customizes fixtures to use the In-Memory database provider when creating *DbContext*
  instances
- `SqliteContextCustomization` - customizes fixtures to use the SQLite database provider when creating *DbContext*
  instances.
  By default the customization will create contexts for an in-memory *connection string* (i.e. `DataSource=:memory:`).
  This can be changed by providing the fixture a predefined `SqliteConnection` instance.
- `DbContextCustomization` - serves as the base customization for the other two implementations. The customization can
  be used, in more advanced scenarios, when you want to extend the fixtures with your own specimen builders.

## Examples

The examples below demonstrate, the possible ways of using the library in [xUnit](https://github.com/xunit/xunit) test
projects, both with `[Fact]` and `[Theory]` tests.

The library is not limited to `xUnit` and can be used with other testing frameworks like `NUnit` and `MSTest`, since it
only provides a few `Customization` implementations.

### Using In-Memory database provider

```csharp
[Fact]
public async Task CanUseGeneratedContext()
{
    // Arrange
    var fixture = new Fixture().Customize(new InMemoryContextCustomization
    {
        AutoCreateDatabase = true,
        OmitDbSets = true
    });
    var context = fixture.Create<TestDbContext>();

    // Act
    context.Customers.Add(new Customer("Jane Smith"));
    await context.SaveChangesAsync();

    // Assert
    context.Customers.Should().Contain(x => x.Name == "Jane Smith");
}
```

The next example uses a custom `AutoData` attribute `AutoDomainDataWithInMemoryContext` that customizes the fixture with
the same customization as in the example above. This helps abstract away even more setup code.

```csharp
[Theory, InMemoryData]
public async Task CanUseGeneratedContext(TestDbContext context)
{
    // Arrange & Act
    context.Customers.Add(new Customer("Jane Smith"));
    await context.SaveChangesAsync();

    // Assert
    context.Customers.Should().Contain(x => x.Name == "Jane Smith");
}
```

The attribute used in the test above might look something like the following.

```csharp
public class InMemoryDataAttribute : AutoDataAttribute
{
    public InMemoryDataAttribute()
        : base(() => new Fixture()
            .Customize(new InMemoryContextCustomization
            {
                OmitDbSets = true,
                AutoCreateDatabase = true
            }))
    {
    }
}
```

### Using SQLite database provider

When using the SQLite database provider there is another configuration available in the customization,
called `AutoOpenConnection` which allows to automatically open database connections, after they are resolved from the
fixture. The option is turned off by default in v1, but might become the default in next major releases.

```csharp
[Fact]
public async Task CanUseGeneratedContext()
{
    // Arrange
    var fixture = new Fixture()
        .Customize(new SqliteContextCustomization
        {
            AutoCreateDatabase = true,
            AutoOpenConnection = true,
            OmitDbSets = true
        });
    var context = fixture.Create<TestDbContext>();

    // Act
    context.Customers.Add(new Customer("Jane Smith"));
    await context.SaveChangesAsync();

    // Assert
    context.Customers.Should().Contain(x => x.Name == "Jane Smith");
}
```

The same test can be written like this, by using a custom data attribute.

```csharp
[Theory, SqliteData]
public void CanUseResolvedContextInstance(TestDbContext context)
{
    // Arrange & Act
    context.Customers.Add(new Customer("Jane Smith"));
    context.SaveChanges();

    // Assert
    context.Customers.Should().Contain(x => x.Name == "Jane Smith");
}
```

```csharp
public class SqliteDataAttribute : AutoDataAttribute
{
    public SqliteDataAttribute()
        : base(() => new Fixture()
            .Customize(new SqliteContextCustomization
            {
                OmitDbSets = true,
                AutoOpenConnection = true,
                AutoCreateDatabase = true
            }))
    {
    }
}
```

## License

Copyright &copy; 2019 [Andrei Ivascu](https://github.com/aivascu).<br/>
This project is [MIT](https://github.com/aivascu/EntityFrameworkCore.AutoFixture/blob/master/LICENSE) licensed.