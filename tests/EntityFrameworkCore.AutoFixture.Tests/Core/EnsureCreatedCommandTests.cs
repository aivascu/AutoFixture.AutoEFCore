using System;
using AutoFixture.Kernel;
using EntityFrameworkCore.AutoFixture.Core;
using EntityFrameworkCore.AutoFixture.Tests.Common;
using EntityFrameworkCore.AutoFixture.Tests.Common.Persistence;
using EntityFrameworkCore.AutoFixture.Tests.Common.Persistence.Entities;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFrameworkCore.AutoFixture.Tests.Core;

public class EnsureCreatedCommandTests
{
    [Fact]
    public void IsCommand()
    {
        typeof(EnsureCreatedCommand)
            .Should().BeAssignableTo<ISpecimenCommand>();
    }

    [Fact]
    public void CanCreateInstance()
    {
        _ = new EnsureCreatedCommand();
    }

    [Fact]
    public void ThrowsWhenRequestNull()
    {
        var command = new EnsureCreatedCommand();

        var act = () => command.Execute(default!, new DelegatingSpecimenContext());

        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void ThrowsWhenRequestNotContext()
    {
        var command = new EnsureCreatedCommand();

        var act = () => command.Execute(new object(), new DelegatingSpecimenContext());

        act.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void RunsEnsureCreated()
    {
        var command = new EnsureCreatedCommand();
        var connection = new SqliteConnection("Data Source=:memory:");
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new TestDbContext(options);
        connection.Open();

        command.Execute(context, default!);

        context.Items.Add(new Item("potato", 1));
        context.SaveChanges();
    }
}
