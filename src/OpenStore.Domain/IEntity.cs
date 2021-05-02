namespace OpenStore.Domain
{
    public interface IEntity
    {
        object Id { get; }
        long Version { get; }
    }
}