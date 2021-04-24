using System;
using System.Collections.Generic;
using OpenStore.Domain.EventSourcing.Exception;
using OpenStore.Domain.EventSourcing.Internal;

namespace OpenStore.Domain.EventSourcing
{
    /// <summary>
    /// An abstraction for event sourced aggregate 
    /// </summary>
    public abstract class EventSourcedAggregateRoot : AggregateRoot<string>
    {
        private readonly object _applyLock = new();
        private readonly object _loadLock = new();
        private readonly Dictionary<Type, string> _eventHandlerCache;

        public sealed override long Version { get; set; }
        public long LastCommittedVersion { get; protected set; }

        protected EventSourcedAggregateRoot()
        {
            Version = (long) StreamState.NoStream;
            LastCommittedVersion = (long) StreamState.NoStream;
            _eventHandlerCache = ReflectionHelper.FindEventHandlerMethodsInAggregate(GetType());
        }

        protected StreamState StreamState => Version == 0 ? StreamState.NoStream : StreamState.HasStream;

        public void Load(IEnumerable<IDomainEvent> events)
        {
            lock (_loadLock)
            {
                foreach (var e in events)
                    ApplyChangeInternal(e, false);

                LastCommittedVersion = Version;
            }
        }

        protected override void ApplyChange(IDomainEvent @event)
        {
            lock (_applyLock)
            {
                ApplyChangeInternal(@event, true);
            }
        }

        private void ApplyChangeInternal(IDomainEvent @event, bool isNew)
        {
            if (CanApply(@event))
            {
                DoApply(@event);

                if (isNew)
                    base.ApplyChange(@event);
                else
                    Version++;
            }
            else
            {
                throw new AggregateStateMismatchException(
                    $"The event target version is {@event.Id}.(Version {@event.Version}) and " +
                    $"AggregateRoot version is {Id}.(Version {Version})");
            }
        }

        private bool CanApply(IDomainEvent @event)
        {
            if (StreamState == StreamState.NoStream)
                return true;

            return Id.Equals(@event.Id) && Version == @event.Version;
        }

        private void DoApply(IDomainEvent @event)
        {
            if (StreamState == StreamState.NoStream)
                Id = @event.Id.ToString();

            if (_eventHandlerCache.ContainsKey(@event.GetType()))
                ReflectionHelper.InvokeOnAggregate(this, _eventHandlerCache[@event.GetType()], @event);
            else
                throw new AggregateEventOnApplyMethodMissingException($"No event handler specified for {@event.GetType()} on {GetType()}");
        }

        public void HydrateFromSnapshot(ISnapshot snapshot)
        {
            Id = snapshot.AggregateId.ToString();
            Version = snapshot.Version;
            LastCommittedVersion = snapshot.Version;
        }
    }
}