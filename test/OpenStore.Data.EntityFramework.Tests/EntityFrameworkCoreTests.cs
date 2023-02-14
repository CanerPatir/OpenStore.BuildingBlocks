using CommonFixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Application;
using OpenStore.Application.Crud;
using OpenStore.Data.OutBox;
using OpenStore.Domain;
using OpenStore.Infrastructure;
using OpenStore.Infrastructure.Mapping.AutoMapper;
using Xunit;

namespace OpenStore.Data.EntityFramework.Tests;

public class EntityFrameworkCoreTests : WithEfCore<TestDbContext>
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();
        services.AddOpenStoreCore(typeof(EntityFrameworkCoreTests).Assembly);
        services.AddOpenStoreObjectMapper(configure => { });
        services.AddOpenStoreEfCore<TestDbContext, TestDbContext>(OpenStoreEntityFrameworkSettings.Default);
    }

    [Fact]
    public void DiResolve()
    {
        // Arrange

        // Act
        var repo = GetService<IRepository<TestAggregate>>();
        var qRepo = GetService<ICrudRepository<TestAggregate>>();
        var tRepo = GetService<ITransactionalRepository<TestAggregate>>();
        var outBoxService = GetService<IOutBoxService>();
        var outBoxStoreService = GetService<IOutBoxStoreService>();
        var uow = GetService<IUnitOfWork>();
        var crudService = GetService<ICrudService<TestAggregate, TestDto>>();

        // Assert
        Assert.NotNull(repo);
        Assert.NotNull(qRepo);
        Assert.NotNull(tRepo);
        Assert.NotNull(outBoxService);
        Assert.NotNull(outBoxStoreService);
        Assert.NotNull(uow);
        Assert.NotNull(crudService);
    }


    [Fact]
    public async Task Create()
    {
        // Arrange
        var repo = GetService<IEntityFrameworkRepository<TestAggregate>>();

        var entity = new TestAggregate("test");
        await repo.SaveAsync(entity);
        // Act

        NewServiceScope();

        var newRepo = GetService<ITransactionalRepository<TestAggregate>>();
        var loadedEntity = await newRepo.GetAsync(entity.Id);

        // Assert
        Assert.NotNull(loadedEntity);
        Assert.True(entity == loadedEntity);
    }

    [Fact]
    public async Task Update()
    {
        // Arrange
        var repo = GetService<ITransactionalRepository<TestAggregate>>();

        var entity = new TestAggregate("test");
        await repo.SaveAsync(entity);

        // Act
        entity.ChangeInventoryCode("mutated");
        await repo.SaveAsync(entity);

        // Assert
        using var scope = NewServiceScope();
        var newRepo = GetService<ITransactionalRepository<TestAggregate>>();

        var lastState = await newRepo.GetAsync(entity.Id);

        Assert.True(lastState.InventoryCode == "mutated");
    }

    [Fact]
    public async Task UpdateEntityWithVersion()
    {
        // Arrange
        var repo = GetService<ICrudRepository<TestEntity>>();

        var entity = new TestEntity();
        await repo.InsertAsync(entity);
        await repo.SaveChangesAsync();

        // Act
        entity.Data = "mutated";
        await repo.SaveChangesAsync();

        // Assert
        using var scope = NewServiceScope();
        var newRepo = GetService<ICrudRepository<TestEntity>>();

        var lastState = await newRepo.GetAsync(entity.Id);

        Assert.True(lastState.Data == "mutated");
        Assert.Equal(1L, lastState.Version);
    }

    [Fact]
    public async Task UpdateEntityWithOutboxMessages()
    {
        // Arrange
        var repo = GetService<ITransactionalRepository<TestAggregate>>();

        var entity = new TestAggregate("test");
        await repo.SaveAsync(entity);

        // Act
        entity.ChangeInventoryCodeAndRegisterEvent("mutated");
        await repo.SaveAsync(entity);

        // Assert
        using var scope = NewServiceScope();
        var newRepo = GetService<ITransactionalRepository<TestAggregate>>();

        var lastState = await newRepo.GetAsync(entity.Id);

        var dbContext = GetService<TestDbContext>();

        Assert.NotEmpty(await dbContext.OutBoxMessages.ToListAsync());
        Assert.True(lastState.InventoryCode == "mutated");
    }

    [Fact]
    public async Task Delete()
    {
        // Arrange
        var repo = GetService<ITransactionalRepository<TestAggregate>>();
        var entity = new TestAggregate("test");
        await repo.SaveAsync(entity);

        // Act
        NewServiceScope();
        repo = GetService<ITransactionalRepository<TestAggregate>>();
        entity = await repo.GetAsync(entity.Id);
        await repo.Delete(entity);
        await repo.SaveAsync(entity);

        // Assert
        NewServiceScope();
        repo = GetService<ITransactionalRepository<TestAggregate>>();

        var lastState = await repo.GetAsync(entity.Id);

        Assert.Null(lastState);
    }

    [Fact]
    public async Task Query_Test()
    {
        // Arrange
        var repo = GetService<IRepository<TestAggregate>>();
        var repo2 = GetService<ICrudRepository<TestAggregate>>();

        await repo.SaveAsync(new TestAggregate("test"));
        await repo.SaveAsync(new TestAggregate("test2"));

        // Act
        var items = await repo2.Query.ToListAsync();

        // Assert
        Assert.NotNull(items);
        Assert.Equal(2, items.Count);
    }
}