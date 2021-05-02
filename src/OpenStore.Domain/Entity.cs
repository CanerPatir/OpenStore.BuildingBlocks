using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

// ReSharper disable once MemberCanBeProtected.Global

namespace OpenStore.Domain
{
    public abstract class Entity<TKey> : IEntity, ISavingChanges
    {
        [Key] public virtual TKey Id { get; protected set; }

        object IEntity.Id => Id;

        [ConcurrencyCheck] public virtual long Version { get; protected set; }

        public override bool Equals(object obj)
        {
            if (obj is not Entity<TKey> other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetType() != other.GetType())
                return false;

            if (Id.Equals(default) || other.Id.Equals(default))
                return false;

            return Id.Equals(other.Id);
        }

        public static bool operator ==(Entity<TKey> a, Entity<TKey> b)
        {
            if (a is null && b is null)
                return true;

            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(Entity<TKey> a, Entity<TKey> b) => !(a == b);

        public override int GetHashCode() => (GetType().ToString() + Id).GetHashCode();

        #region ISavingChanges memebers

        private readonly List<IDomainEvent> _uncommittedChanges = new();

        IReadOnlyCollection<IDomainEvent> ISavingChanges.GetUncommittedChanges() => _uncommittedChanges;

        bool ISavingChanges.HasUncommittedChanges() => _uncommittedChanges.Any();

        void ISavingChanges.Commit() => _uncommittedChanges.Clear();
        void ISavingChanges.SetVersionExplicitly(long version) => Version = version;

        protected virtual void ApplyChange(IDomainEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            lock (_uncommittedChanges)
            {
                IncreaseVersion();
                @event.Version = Version;
                _uncommittedChanges.Add(@event);
            }
        }

        private void IncreaseVersion()
        {
            Version++;
            _versionIncreasedAtLeastOne = true;
        }

        void ISavingChanges.OnSavingChanges()
        {
            if (_versionIncreasedAtLeastOne) return;
            IncreaseVersion();
        }

        private bool _versionIncreasedAtLeastOne;

        #endregion
    }
}