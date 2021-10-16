using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Application;
using OpenStore.Domain;
using CommonFixtures;
using OpenStore.Application.Crud;
using OpenStore.Data.OutBox;
using Raven.Client.Documents;
using Raven.Client.ServerWide.Operations;
using Raven.Embedded;
using Xunit;
using OpenStore.Infrastructure;
using OpenStore.Infrastructure.Mapping.AutoMapper;

namespace OpenStore.Data.NoSql.RavenDb.Tests;

[CollectionDefinition("RavenEmbeddedCrudTest", DisableParallelization = true)]
public class SequentialCollection
{
}

[Collection("RavenEmbeddedCrudTest")]
public class RavenDbTests : WithIoC
{
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
        
    public class TestDto
    {
            
    }

    private const string TestStoreName = "Test";

    protected override void ConfigureServices(IServiceCollection services)
    {
        EmbeddedServer.Instance.StartServer();
        var testStore = EmbeddedServer.Instance.GetDocumentStore(TestStoreName);
        services.AddLogging();
        services.AddOpenStoreCore(typeof(RavenDbTests).Assembly);
        services.AddRavenDbDataInfrastructure(options =>
        {
            options.OutBoxEnabled = false;
        });
        services.AddOpenStoreObjectMapper(configure => { });
        services.AddSingleton<IDocumentStore>(testStore);
    }

    [Fact(Skip = "s")]
    public void DiResolve()
    {

        // Arrange

        // Act
        var repo = GetService<IRepository<Test>>();
        var ravenRepo = GetService<IRavenRepository<Test>>();
        var qRepo = GetService<ICrudRepository<Test>>();
        var tRepo = GetService<ITransactionalRepository<Test>>();
        var outBoxService = GetService<IOutBoxService>();
        var outBoxStoreService = GetService<IOutBoxStoreService>();
        var uow = GetService<IUnitOfWork>();
        var crudService = GetService<ICrudService<Test, TestDto>>();

        // Assert
        Assert.NotNull(repo);
        Assert.NotNull(ravenRepo);
        Assert.NotNull(qRepo);
        Assert.NotNull(tRepo);
        Assert.NotNull(outBoxService);
        Assert.NotNull(outBoxStoreService);
        Assert.NotNull(uow);
        Assert.NotNull(crudService);
    }

        
    [Fact(Skip = "s")]
    public async Task Create()
    {
        // Arrange
        var repo = GetService<IRavenRepository<Test>>();

        var id = Guid.NewGuid();
        var entity = new Test("myId", "test");
        await repo.SaveAsync(entity);
        // Act

        NewServiceScope();

        var newRepo = GetService<IRavenRepository<Test>>();
        var loadedEntity = await newRepo.GetAsync(entity.Id);

        // Assert
        Assert.NotNull(loadedEntity);
        Assert.True(entity == loadedEntity);
    }

    [Fact(Skip = "s")]
    public async Task Update()
    {
        // Arrange
        var repo = GetService<IRavenRepository<Test>>();

        var id = Guid.NewGuid();
        var entity = new Test("myId", "test");
        await repo.SaveAsync(entity);

        // Act
        entity.InventoryCode = "mutated";
        await repo.SaveAsync(entity);

        // Assert
        using var scope = NewServiceScope();
        var newRepo = GetService<IRavenRepository<Test>>();

        var lastState = await newRepo.GetAsync(entity.Id);

        Assert.True(lastState.InventoryCode == "mutated");
    }

    [Fact(Skip = "s")]
    public async Task Delete()
    {
        // Arrange
        var repo = GetService<IRavenRepository<Test>>();
        var entity = new Test("MyId", "test");
        await repo.SaveAsync(entity);

        // Act
        NewServiceScope();
        repo = GetService<IRavenRepository<Test>>();
        var uow = GetService<IUnitOfWork>();
            
        entity = await repo.GetAsync(entity.Id);
        await repo.Delete(entity);
        await uow.SaveChangesAsync();

        // Assert
        NewServiceScope();
        repo = GetService<IRavenRepository<Test>>();

        var lastState = await repo.GetAsync(entity.Id);

        Assert.Null(lastState);
    }

    [Fact(Skip = "s")]
    public async Task Query()
    {
        // Arrange
        var repo = GetService<IRavenRepository<Test>>();
        var repo2 = GetService<ICrudRepository<Test>>();

        var id = Guid.NewGuid();
        await repo.SaveAsync(new Test("MyId1", "test"));
        await repo.SaveAsync(new Test("MyId2", "test2"));

        // Act
        var items = await repo2.Query.ToListAsync();

        // Assert

        Assert.NotNull(items);
        Assert.Equal(2, items.Count);
    }

    public override void Dispose()
    {
        base.Dispose();

        var parameters = new DeleteDatabasesOperation.Parameters
        {
            DatabaseNames = new[] {TestStoreName},
            HardDelete = true,
        };
        EmbeddedServer.Instance.GetDocumentStore(TestStoreName).Maintenance.Server.Send(new DeleteDatabasesOperation(parameters));
        EmbeddedServer.Instance.Dispose();
    }
}