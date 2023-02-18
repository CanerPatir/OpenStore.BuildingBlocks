using CommonFixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Application;
using OpenStore.Application.Crud;
using OpenStore.Data.EntityFramework.ReadOnly;
using OpenStore.Data.OutBox;
using OpenStore.Domain;
using OpenStore.Infrastructure;
using OpenStore.Infrastructure.Mapping.AutoMapper;
using Xunit;

namespace OpenStore.Data.EntityFramework.Tests;

public class EntityFrameworkCoreWithReadReplicaTests : WithEfCore<TestDbContext>
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();
        services.AddOpenStoreCore(typeof(EntityFrameworkCoreWithReadReplicaTests).Assembly);
        services.AddOpenStoreObjectMapper(configure => { });

        var testEfSettings = new OpenStoreEntityFrameworkSettings(
            EntityFrameworkDataSource.SqLite,
            new OpenStoreEntityFrameworkSettingsConnectionStrings(
                "Filename=:memory:",
                "Filename=:memory:"
            ),
            true);
        services.AddOpenStoreEfCoreWithReadReplica<TestDbContext, TestDbContext, TestReadOnlyDbContext, TestReadOnlyDbContext>(testEfSettings);
    }


    [Fact]
    public void DiResolve()
    {
        // Arrange

        // Act
        var repo = GetService<IRepository<TestAggregate>>();
        var readOnlyRepository = GetService<IReadOnlyRepository<TestEntity>>();
        var qRepo = GetService<ICrudRepository<TestAggregate>>();
        var tRepo = GetService<ITransactionalRepository<TestAggregate>>();
        var outBoxService = GetService<IOutBoxService>();
        var outBoxStoreService = GetService<IOutBoxStoreService>();
        var uow = GetService<IUnitOfWork>();
        var dbContext = GetService<TestDbContext>();
        var testReadOnlyDbContext = GetService<TestReadOnlyDbContext>();
        var readOnlyDbContext = GetService<ReadOnlyDbContext>();
        var iReadOnlyDbContext = GetService<IReadOnlyDbContext>();
        var crudService = GetService<ICrudService<TestAggregate, TestDto>>();

        // Assert
        Assert.NotNull(repo);
        Assert.NotNull(readOnlyRepository);
        Assert.NotNull(qRepo);
        Assert.NotNull(tRepo);
        Assert.NotNull(outBoxService);
        Assert.NotNull(outBoxStoreService);
        Assert.NotNull(uow);
        Assert.NotNull(dbContext);
        Assert.NotNull(testReadOnlyDbContext);
        Assert.NotNull(readOnlyDbContext);
        Assert.NotNull(iReadOnlyDbContext);
        Assert.NotNull(crudService);
    }
}