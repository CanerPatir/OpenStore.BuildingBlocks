using System.Collections.Generic;

namespace OpenStore.Domain
{
    public interface IAggregateRoot : IEntity
    {
        IReadOnlyCollection<IDomainEvent> GetUncommittedChanges();
        bool HasUncommittedChanges();
        void Commit();
    }
}