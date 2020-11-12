using Microsoft.EntityFrameworkCore;
using OpenStore.Domain;

namespace OpenStore.Infrastructure.Data.EntityFramework.Tests
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; }
    }
    
    public class TestEntity : AggregateRoot<int>
    {
        public string InventoryCode { get; protected set; }

        protected TestEntity()
        {
        }

        public TestEntity(string inventoryCode)
        {
            InventoryCode = inventoryCode;
        }

        public void ChangeInventoryCode(string inventoryCode)
        {
            InventoryCode = inventoryCode;
        }
    }
}