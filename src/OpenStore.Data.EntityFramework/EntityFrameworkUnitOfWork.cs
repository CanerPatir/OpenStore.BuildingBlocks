using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OpenStore.Data.EntityFramework.Extensions;
using OpenStore.Data.OutBox;

namespace OpenStore.Data.EntityFramework;

public class EntityFrameworkUnitOfWork<TDbContext> : IEntityFrameworkCoreUnitOfWork, IDisposable
    where TDbContext : DbContext
{
    private IDbContextTransaction _tx;
    public DbContext Context { get; }
    public IOutBoxStoreService OutBoxStoreService { get; }

    public EntityFrameworkUnitOfWork(TDbContext dbContext, IOutBoxStoreService outBoxStoreService)
    {
        Context = dbContext;
        OutBoxStoreService = outBoxStoreService;
    }

    public async Task BeginTransactionAsync(CancellationToken token = default)
    {
        _tx = Context.Database.CurrentTransaction ?? await Context.Database.BeginTransactionAsync(token);
    }

    public async Task SaveChangesAsync(CancellationToken token = default)
    {
        if (_tx != null)
        {
            await _tx.CommitAsync(token);
        }

        var result = await Context.SaveChangesWithValidationAsync(OutBoxStoreService, token: token);

        if (!result.IsValid)
        {
            throw new EntityValidationException(result.Message, result.Errors.Select(x => x.ErrorResult));
        }
    }

    public void Dispose()
    {
        _tx?.Dispose();
    }
}