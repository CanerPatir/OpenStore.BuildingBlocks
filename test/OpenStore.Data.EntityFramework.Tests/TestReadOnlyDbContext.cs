using Microsoft.EntityFrameworkCore;
using OpenStore.Data.EntityFramework.ReadOnly;
using OpenStore.Data.OutBox;

namespace OpenStore.Data.EntityFramework.Tests;

public class TestReadOnlyDbContext : ReadOnlyDbContext
{
    public TestReadOnlyDbContext(DbContextOptions<TestReadOnlyDbContext> options) : base(options)
    {
    }

    public DbSet<TestAggregate> TestAggregates { get; set; }
    public DbSet<TestEntity> TestEntities { get; set; }
}