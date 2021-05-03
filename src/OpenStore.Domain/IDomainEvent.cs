namespace OpenStore.Domain
{
    public interface IDomainEvent
    {
        string Id { get; }
        long Version { get; set; }
    }
}