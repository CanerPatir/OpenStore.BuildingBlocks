using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenStore.Application;
using OpenStore.Domain;

namespace OpenStore.Data.OutBox;

public abstract class OutBoxStoreService : IOutBoxStoreService
{
    private IOpenStoreUserContextAccessor UserContextAccessor { get; }

    protected OutBoxStoreService(IOpenStoreUserContextAccessor userContextAccessor)
    {
        UserContextAccessor = userContextAccessor;
    }

    protected IEnumerable<OutBoxMessage> WrapEvents(IEnumerable<IDomainEvent> events)
    {
        var correlationId = Guid.NewGuid().ToString();
        var committedBy = UserContextAccessor.GetUserEmail();
        return events.Select(e => new OutBoxMessage(e, correlationId, committedBy));
    }

    public abstract Task StoreMessages(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
}