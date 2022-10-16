using Microsoft.EntityFrameworkCore;
using OpenStore.Application;
using OpenStore.Data.OutBox;
using OpenStore.Domain;

namespace OpenStore.Data.EntityFramework.OutBox;

public class EntityFrameworkOutBoxStoreService<TDbContext> : OutBoxStoreService
    where TDbContext : DbContext
{
    private readonly bool _outBoxEnabled;
    private readonly DbContext _context;

    public EntityFrameworkOutBoxStoreService(
        bool outBoxEnabled,
        TDbContext context,
        IOpenStoreUserContextAccessor openStoreUserContextAccessor) : base(openStoreUserContextAccessor)
    {
        _outBoxEnabled = outBoxEnabled;
        _context = context;
    }

    public override async Task StoreMessages(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        if (_outBoxEnabled is false)
        {
            return;
        }

        if (_context is IOutBoxDbContext eventStoreContext)
        {
            await eventStoreContext.OutBoxMessages.AddRangeAsync(WrapEvents(events), cancellationToken);
        }
    }
}