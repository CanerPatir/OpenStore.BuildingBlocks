using System;
using System.Threading.Tasks;
using CommonFixtures;
using Microsoft.Extensions.DependencyInjection;
using Mongo2Go;
using MongoDB.Driver;
using OpenStore.Application;
using OpenStore.Application.Crud;
using OpenStore.Domain;
using OpenStore.Infrastructure.CommandBus;
using Xunit;

namespace OpenStore.Infrastructure.Data.NoSql.MongoDb.Tests
{
    public class MongoDbTests : WithIoC
    {
        private MongoDbRunner _runner;

        public sealed class Test : AggregateRoot<string>
        {
            public string InventoryCode { get; set; }

            public Test(string id, string inventoryCode)
            {
                Id = id;
                InventoryCode = inventoryCode;
            }

            public Test()
            {
            }
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            _runner = MongoDbRunner.Start();

            services.AddLogging();
            services.AddOpenStoreCommandBus<MongoDbTests>();
            services.AddMongoDbDataInfrastructure(options => { }, "IntegrationTest");
            services.AddSingleton(sp => new MongoClient(_runner.ConnectionString));
        }

        public override void Dispose()
        {
            base.Dispose();
            _runner?.Dispose();
        }

        [Fact]
        public void DiResolve()
        {
            // Arrange

            // Act
            var repo = GetService<IRepository<Test>>();
            var ravenRepo = GetService<IMongoRepository<Test>>();
            var qRepo = GetService<ICrudRepository<Test>>();
            var tRepo = GetService<ITransactionalRepository<Test>>();

            // Assert
            Assert.NotNull(repo);
            Assert.NotNull(ravenRepo);
            Assert.NotNull(qRepo);
            Assert.NotNull(tRepo);
        }

        [Fact]
        public async Task Create()
        {
            // Arrange
            var repo = GetService<IRepository<Test>>();

            var id = Guid.NewGuid();
            var entity = new Test("myId", "test");
            await repo.SaveAsync(entity);
            // Act

            NewServiceScope();

            var newRepo = GetService<IRepository<Test>>();
            var loadedEntity = await newRepo.GetAsync(entity.Id);

            // Assert
            Assert.NotNull(loadedEntity);
            Assert.True(entity == loadedEntity);
        }

        [Fact]
        public async Task Update()
        {
            // Arrange
            var repo = GetService<IRepository<Test>>();

            var id = Guid.NewGuid();
            var entity = new Test("myId", "test");
            await repo.SaveAsync(entity);

            // Act
            entity.InventoryCode = "mutated";
            await repo.SaveAsync(entity);

            // Assert
            using var scope = NewServiceScope();
            var newRepo = GetService<IRepository<Test>>();

            var lastState = await newRepo.GetAsync(entity.Id);

            Assert.True(lastState.InventoryCode == "mutated");
        }

        [Fact]
        public async Task Delete()
        {
            // Arrange
            var repo = GetService<IRepository<Test>>();
            var entity = new Test("MyId", "test");
            await repo.SaveAsync(entity);

            // Act
            NewServiceScope();
            repo = GetService<IRepository<Test>>();
            var uow = GetService<IUnitOfWork>();
            entity = await repo.GetAsync(entity.Id);
            await repo.Delete(entity);
            await uow.SaveChangesAsync();
            // await repo.SaveAsync(entity);

            // Assert
            NewServiceScope();
            repo = GetService<IRepository<Test>>();

            var lastState = await repo.GetAsync(entity.Id);

            Assert.Null(lastState);
        }

        [Fact]
        public async Task Query()
        {
            // Arrange
            var repo = GetService<IRepository<Test>>();

            var id = Guid.NewGuid();
            await repo.SaveAsync(new Test("MyId1", "test"));
            await repo.SaveAsync(new Test("MyId2", "test2"));

            NewServiceScope();
            var repo2 = GetService<IMongoRepository<Test>>();
            // Act
            var items = await repo2.MongoQuery.ToListAsync();

            // Assert

            Assert.NotNull(items);
            Assert.Equal(2, items.Count);
        }
    }
}