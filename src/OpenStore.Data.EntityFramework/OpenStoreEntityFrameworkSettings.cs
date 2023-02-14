// ReSharper disable ClassNeverInstantiated.Global

namespace OpenStore.Data.EntityFramework;

public record OpenStoreEntityFrameworkSettings(
    EntityFrameworkDataSource ActiveConnection,
    OpenStoreEntityFrameworkSettingsConnectionStrings ConnectionStrings,
    bool OutBoxEnabled,
    int OutBoxFetchSize = 2000
);

public record OpenStoreEntityFrameworkSettingsConnectionStrings(
    string Default,
    string? ReadReplica
);