using Microsoft.EntityFrameworkCore;
using OpenStore.Application;
using OpenStore.Data.OutBox;
using OpenStore.Domain;

namespace OpenStore.Data.EntityFramework.OutBox;

public class EntityFrameworkOutBoxStoreService : OutBoxStoreService
{
    private readonly DbContext _context;

    public EntityFrameworkOutBoxStoreService(
        DbContext context,
        IOpenStoreUserContextAccessor openStoreUserContextAccessor) : base(openStoreUserContextAccessor)
    {
        _context = context;
    }

    public override async Task StoreMessages(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        if (_context is IOutBoxDbContext eventStoreContext)
        {
            await eventStoreContext.OutBoxMessages.AddRangeAsync(WrapEvents(events), cancellationToken);
        }
    }
}