using Storage.Net.Amazon.Aws.Blobs;
using Storage.Net.Blobs;
using Storage.Net.Microsoft.Azure.Storage.Blobs;

namespace OpenStore.Infrastructure.OSS;

public class StorageNetOssStore : IStorageNetOssStore
{
    public StorageNetOssStore(IBlobStorage blobStorage)
    {
        BlobStorage = blobStorage ?? throw new ArgumentNullException(nameof(blobStorage));
    }

    public IBlobStorage BlobStorage { get; }

    public IAwsS3BlobStorage AsAwsS3()
    {
        if (BlobStorage is IAwsS3BlobStorage awsS3BlobStorage)
            return awsS3BlobStorage;

        throw new NotSupportedException("Aws s3 not supported");
    }

    public IAzureBlobStorage AsAzureBlobStorage()
    {
        if (BlobStorage is IAzureBlobStorage azureBlobStorage)
            return azureBlobStorage;

        throw new NotSupportedException("Azure blob storage not supported");
    }

    public Task WriteAsync(string fullPath, Stream dataStream, bool append = false, CancellationToken cancellationToken = default)
        => BlobStorage.WriteAsync(fullPath, dataStream, append, cancellationToken);

    public Task<Stream> OpenReadAsync(string fullPath, CancellationToken cancellationToken = default)
        => BlobStorage.OpenReadAsync(fullPath, cancellationToken);

    public Task DeleteAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default)
        => BlobStorage.DeleteAsync(fullPaths, cancellationToken);

    public Task<IReadOnlyCollection<bool>> ExistsAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default)
        => BlobStorage.ExistsAsync(fullPaths, cancellationToken);
}