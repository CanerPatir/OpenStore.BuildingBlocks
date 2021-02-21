using System.Threading.Tasks;
using CommonFixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Application;
using OpenStore.Application.Crud;
using OpenStore.Domain;
using OpenStore.Infrastructure.CommandBus;
using Xunit;

namespace OpenStore.Infrastructure.Data.EntityFramework.Tests
{
    public class EntityFrameworkCoreTests : WithEfCore<TestDbContext>
    {
        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddOpenStoreCommandBus<EntityFrameworkCoreTests>();
            services.AddOpenStoreEfCore<TestDbContext, TestDbContext>("test conn str", EntityFrameworkDataSource.PostgreSql);
        }

        [Fact]
        public void DiResolve()
        {
            // Arrange

            // Act
            var repo = GetService<IRepository<TestEntity>>();
            var qRepo = GetService<ICrudRepository<TestEntity>>();
            var tRepo = GetService<ITransactionalRepository<TestEntity>>();

            // Assert
            Assert.NotNull(repo);
            Assert.NotNull(qRepo);
            Assert.NotNull(tRepo);
        }
        
        
        [Fact]
        public async Task Create()
        {
            // Arrange
            var repo = GetService<IEntityFrameworkRepository<TestEntity>>();

            var entity = new TestEntity("test");
            await repo.SaveAsync(entity);
            // Act

            NewServiceScope();

            var newRepo = GetService<ITransactionalRepository<TestEntity>>();
            var loadedEntity = await newRepo.GetAsync(entity.Id);

            // Assert
            Assert.NotNull(loadedEntity);
            Assert.True(entity == loadedEntity);
        }

        [Fact]
        public async Task Update()
        {
            // Arrange
            var repo = GetService<ITransactionalRepository<TestEntity>>();

            var entity = new TestEntity("test");
            await repo.SaveAsync(entity);

            // Act
            entity.ChangeInventoryCode("mutated");
            await repo.SaveAsync(entity);

            // Assert
            using var scope = NewServiceScope();
            var newRepo = GetService<ITransactionalRepository<TestEntity>>();

            var lastState = await newRepo.GetAsync(entity.Id);

            Assert.True(lastState.InventoryCode == "mutated");
        }

        [Fact]
        public async Task Delete()
        {
            // Arrange
            var repo = GetService<ITransactionalRepository<TestEntity>>();
            var entity = new TestEntity("test");
            await repo.SaveAsync(entity);

            // Act
            NewServiceScope();
            repo = GetService<ITransactionalRepository<TestEntity>>();
            entity = await repo.GetAsync(entity.Id);
            await repo.Delete(entity);
            await repo.SaveAsync(entity);

            // Assert
            NewServiceScope();
            repo = GetService<ITransactionalRepository<TestEntity>>();

            var lastState = await repo.GetAsync(entity.Id);

            Assert.Null(lastState);
        }

        [Fact]
        public async Task Query_Test()
        {
            // Arrange
            var repo = GetService<IRepository<TestEntity>>();
            var repo2 = GetService<ICrudRepository<TestEntity>>();

            await repo.SaveAsync(new TestEntity("test"));
            await repo.SaveAsync(new TestEntity("test2"));

            // Act
            var items = await repo2.Query.ToListAsync();

            // Assert
            Assert.NotNull(items);
            Assert.Equal(2, items.Count);
        }

    }
}