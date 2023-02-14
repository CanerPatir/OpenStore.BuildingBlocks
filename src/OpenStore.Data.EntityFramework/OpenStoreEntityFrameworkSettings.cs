// ReSharper disable ClassNeverInstantiated.Global

namespace OpenStore.Data.EntityFramework;

public record OpenStoreEntityFrameworkSettings(
    EntityFrameworkDataSource ActiveConnection,
    OpenStoreEntityFrameworkSettingsConnectionStrings ConnectionStrings = null,
    bool OutBoxEnabled = false,
    int OutBoxFetchSize = 2000
);

public record OpenStoreEntityFrameworkSettingsConnectionStrings(
    string Default,
    string? ReadReplica
);