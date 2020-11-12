using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenStore.Domain
{
    /// <summary>
    /// An abstraction for Aggregate root entity which holds and supplies changes on the object. It is feasible to use for document store alse can help in out-of-box pattern scenarios to supply changes on the objects 
    /// </summary>
    public abstract class AggregateRoot<TKey> : Entity<TKey>, IAggregateRoot
    {
        private readonly List<IDomainEvent> _uncommittedChanges = new List<IDomainEvent>();

        public IReadOnlyCollection<IDomainEvent> GetUncommittedChanges() => _uncommittedChanges;

        public bool HasUncommittedChanges() => _uncommittedChanges.Any();

        protected virtual bool TrackChanges { get; } = true;

        public void Commit() => _uncommittedChanges.Clear();

        protected virtual void ApplyChange(IDomainEvent @event)
        {
            if (!TrackChanges) return;
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            lock (_uncommittedChanges)
            {
                Version++;
                @event.Version = Version;
                _uncommittedChanges.Add(@event);
            }
        }

        protected void Fail(string message)
        {
            throw new DomainException(message);
        }
        
        protected void Fail(string message, string errorCode)
        {
            throw new DomainException(message, errorCode);
        }
    }
}