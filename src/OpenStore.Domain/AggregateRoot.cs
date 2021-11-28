namespace OpenStore.Domain;

/// <summary>
/// An abstraction for Aggregate root entity which holds and supplies changes on the object. It is feasible to use for document store alse can help in out-of-box pattern scenarios to supply changes on the objects 
/// </summary>
public abstract class AggregateRoot<TKey> : Entity<TKey>, IAggregateRoot
{
        
}