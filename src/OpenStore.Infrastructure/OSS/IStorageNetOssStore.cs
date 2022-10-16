using OpenStore.Application.ObjectStorage;
using Storage.Net.Amazon.Aws.Blobs;
using Storage.Net.Blobs;
using Storage.Net.Microsoft.Azure.Storage.Blobs;

namespace OpenStore.Infrastructure.OSS;

/// <summary>
/// Provides Storage.Net storage in addition storage abstraction
/// </summary>
public interface IStorageNetOssStore : IOpenStoreOss
{
    public IBlobStorage BlobStorage { get; }

    public IAwsS3BlobStorage AsAwsS3();

    public IAzureBlobStorage AsAzureBlobStorage();
}