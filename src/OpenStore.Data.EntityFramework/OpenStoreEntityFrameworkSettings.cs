// ReSharper disable ClassNeverInstantiated.Global

namespace OpenStore.Data.EntityFramework;

public record OpenStoreEntityFrameworkSettings(
    EntityFrameworkDataSource ActiveConnection,
    OpenStoreEntityFrameworkSettingsConnectionStrings ConnectionStrings,
    bool OutBoxEnabled,
    int OutBoxFetchSize = 2000
)
{
    public static readonly OpenStoreEntityFrameworkSettings Default =
        new(
            EntityFrameworkDataSource.SqLite,
            new OpenStoreEntityFrameworkSettingsConnectionStrings(
                "",
                null
            ),
            true);
};

public record OpenStoreEntityFrameworkSettingsConnectionStrings(
    string Default,
    string? ReadReplica
);