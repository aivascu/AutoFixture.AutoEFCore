using System;
using System.Linq;
using AutoFixture;
using AutoFixture.Kernel;
using EntityFrameworkCore.AutoFixture.Core;
using EntityFrameworkCore.AutoFixture.Tests.Common;
using EntityFrameworkCore.AutoFixture.Tests.Common.Persistence;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace EntityFrameworkCore.AutoFixture.Tests.Core;

public class OptionsBuilderConfiguratorTests
{
    [Fact]
    public void CanCreateInstance()
    {
        _ = new OptionsBuilderConfigurator(new DelegatingBuilder(), builder => builder);
    }

    [Fact]
    public void PropertiesSetInConstructor()
    {
        var next = new DelegatingBuilder();
        var configure = (DbContextOptionsBuilder x) => x;
        var builder = new OptionsBuilderConfigurator(next, configure);

        using (new AssertionScope())
        {
            builder.Builder.Should().BeSameAs(next);
            builder.Configure.Should().BeSameAs(configure);
        }
    }

    [Fact]
    public void ForwardsResultIfNotBuilder()
    {
        var expected = new object();
        var next = new DelegatingBuilder { OnCreate = (_,_) => expected };
        var builder = new OptionsBuilderConfigurator(next, builder => builder);

        var actual = builder.Create(new object(), null!);

        actual.Should().BeSameAs(expected);
    }
    
    [Fact]
    public void ForwardsResultWhenConfigureNull()
    {
        var expected = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("TestDb");
        var next = new DelegatingBuilder { OnCreate = (_,_) => expected };
        var builder = new OptionsBuilderConfigurator(next);

        var actual = builder.Create(new object(), null!);

        actual.Should().BeSameAs(expected);
    }
    
    [Fact]
    public void ReturnsConfiguredBuilder()
    {
        var extensionType = typeof(InMemoryDbContextOptionsExtensions).Assembly
            .FindTypesByName("InMemoryOptionsExtension")
            .FirstOrDefault();
        var configure = (DbContextOptionsBuilder x) => x
            .UseInMemoryDatabase("TestDb");
        var next = new DelegatingBuilder
        {
            OnCreate = (_,_) => new DbContextOptionsBuilder<TestDbContext>()
        };
        var builder = new OptionsBuilderConfigurator(next, configure);

        var actual = (DbContextOptionsBuilder<TestDbContext>)builder.Create(new object(), null!);

        actual.Options.Extensions.Should().Contain(x => x.GetType() == extensionType);
    }
}
