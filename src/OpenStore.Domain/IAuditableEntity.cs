namespace OpenStore.Domain;

public interface IAuditableEntity : IEntity
{
    DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
}