namespace OpenStore.Domain.EventSourcing;

public interface ISnapshottable<TSnapshot> 
{
    TSnapshot TakeSnapshot();
    void ApplySnapshot(TSnapshot snapshot);
}