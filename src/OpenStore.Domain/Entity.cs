using System.ComponentModel.DataAnnotations;

// ReSharper disable once MemberCanBeProtected.Global

namespace OpenStore.Domain
{
    public abstract class Entity<TKey> : IEntity, ISavingChanges
    {
        [Key] public virtual TKey Id { get; protected set; }

        object IEntity.Id => Id;

        [ConcurrencyCheck] public virtual long Version { get; set; }

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
        
        void ISavingChanges.OnSavingChanges() => Version++;
    }
}