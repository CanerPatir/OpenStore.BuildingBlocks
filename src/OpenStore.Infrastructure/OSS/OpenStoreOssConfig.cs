using System.IO;
using Amazon.S3;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace OpenStore.Infrastructure.OSS
{
    public class OpenStoreOssConfig
    {
        public OpenStoreOssProvider Provider { get; set; }
        public FileSystemOssConfig FileSystem { get; set; } = new FileSystemOssConfig();
        public AwsS3OssConfig AwsS3 { get; set; } = new AwsS3OssConfig();
        public AzureOssConfig Azure { get; set; } = new AzureOssConfig();
        // public GoogleCloudStorageOssConfig GoogleCloudStorage { get; set; } = new GoogleCloudStorageOssConfig();
    }

    public enum OpenStoreOssProvider
    {
        FileSystem,
        AwsS3,
        Azure,
        GoogleCloudStorage
    }

    public class FileSystemOssConfig
    {
        public string DirectoryFullName { get; set; }

        public FileSystemOssConfig()
        {
            DirectoryFullName = Path.Combine(Directory.GetCurrentDirectory(), "Oss");
        }
    }

    public class AwsS3OssConfig
    {
        public string AccessKeyId { get; set; }
        public string SecretAccessKey { get; set; }
        public string SessionToken { get; set; }
        public string BucketName { get; set; }
        public AmazonS3Config AmazonS3Config { get; set; } = new AmazonS3Config();
    }

    public class AzureOssConfig
    {
        public string AzureBlobConnectionString { get; set; }
    }

    // public class GoogleCloudStorageOssConfig
    // {
    //     public string BucketName { get; set; }
    //     public string CredentialsJsonString { get; set; }
    //     public bool IsBase64EncodedString { get; set; }
    // }
}