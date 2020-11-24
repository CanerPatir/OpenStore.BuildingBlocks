namespace OpenStore.Domain
{
    public interface IEntity
    {
        object Id { get; }
        ulong Version { get; set; }
    }
}