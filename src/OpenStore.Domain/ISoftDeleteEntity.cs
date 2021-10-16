namespace OpenStore.Domain;

public interface ISoftDeleteEntity : IEntity
{
    bool SoftDeleted { get; set; }
}