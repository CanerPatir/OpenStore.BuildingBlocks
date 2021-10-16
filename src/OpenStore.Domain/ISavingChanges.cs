using System.Collections.Generic;

namespace OpenStore.Domain;

public interface ISavingChanges
{
    IReadOnlyCollection<IDomainEvent> GetUncommittedChanges();
    bool HasUncommittedChanges();
    void Commit();
    void SetVersionExplicitly(long version);
    void OnSavingChanges();
}