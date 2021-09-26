using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using OpenStore.Domain;

namespace OpenStore.Data.EntityFramework.Tests
{
    public class TestDbContext : DbContext, IOutBoxDbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        public DbSet<TestAggregate> TestAggregates { get; set; }
        public DbSet<TestEntity> TestEntities { get; set; }
        public DbSet<OutBoxMessage> OutBoxMessages { get; set; }
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

        public void ChangeInventoryCodeAndRegisterEvent(string inventoryCode)
        {
            InventoryCode = inventoryCode;

            ApplyChange(new InventoryCodeChanged(Id, inventoryCode));
        }
    }

    public class TestDto
    {
    }

    public record InventoryCodeChanged(int AggregateId, string Code) : DomainEvent(AggregateId.ToString());

    public class TestEntity : Entity<int>
    {
        public string Data { get; set; }
    }
}