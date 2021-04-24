using Microsoft.EntityFrameworkCore;
using OpenStore.Domain;

namespace OpenStore.Infrastructure.Data.EntityFramework.Tests
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        public DbSet<TestAggregate> TestAggregates { get; set; }
        public DbSet<TestEntity> TestEntities { get; set; }
    }
    
    public class TestAggregate : AggregateRoot<int>
    {
        public string InventoryCode { get; protected set; }

        protected TestAggregate()
        {
        }

        public TestAggregate(string inventoryCode)
        {
            InventoryCode = inventoryCode;
        }

        public void ChangeInventoryCode(string inventoryCode)
        {
            InventoryCode = inventoryCode;
        }
    }
    
    public class TestEntity : Entity<int>
    {
        public string Data { get; set; }

    }
}