using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Application.ObjectStorage;
using Storage.Net;
using Storage.Net.Blobs;

namespace OpenStore.Infrastructure.OSS
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenStoreOss(this IServiceCollection services, IConfiguration configuration, string configKey = "Oss")
        {
            services.Configure<OpenStoreOssConfig>(configuration.GetSection(configKey));
            var openStoreOssConfig = configuration.GetValue(configKey, new OpenStoreOssConfig());

            return services.AddOpenStoreOss(openStoreOssConfig);
        }
        
        public static IServiceCollection AddOpenStoreOss(this IServiceCollection services, OpenStoreOssConfig config)
        {
            return config.Provider switch
            {
                OpenStoreOssProvider.FileSystem => services.AddFileSystemOss(config.FileSystem),
                OpenStoreOssProvider.AwsS3 => services.AddS3Oss(config.AwsS3),
                OpenStoreOssProvider.Azure => services.AddAzureOss(config.Azure),
                OpenStoreOssProvider.GoogleCloudStorage => services.AddGoogleCloudOss(config.GoogleCloudStorage),
                _ => throw new ArgumentException("config.Provider invalid")
            };
        }

        private static IServiceCollection AddS3Oss(this IServiceCollection services, AwsS3OssConfig config)
        {
            var storage = StorageFactory
                .Blobs
                .AwsS3(config.AccessKeyId, config.SecretAccessKey, config.SessionToken, config.BucketName, config.AmazonS3Config);

            AddDefaults(services, storage);
            return services;
        }

        private static IServiceCollection AddAzureOss(this IServiceCollection services, AzureOssConfig config)
        {
            var storage = StorageFactory
                .Blobs
                .FromConnectionString(config.AzureBlobConnectionString);

            AddDefaults(services, storage);
            return services;
        }

        private static IServiceCollection AddGoogleCloudOss(this IServiceCollection services, GoogleCloudStorageOssConfig config)
        {
            var storage = StorageFactory
                .Blobs
                .GoogleCloudStorageFromJson(config.BucketName, config.CredentialsJsonString, config.IsBase64EncodedString);

            AddDefaults(services, storage);
            return services;
        }

        private static IServiceCollection AddFileSystemOss(this IServiceCollection services, FileSystemOssConfig config)
        {
            var storage = StorageFactory
                .Blobs
                .DirectoryFiles(config.DirectoryFullName);
            AddDefaults(services, storage);
            return services;
        }

        private static void AddDefaults(IServiceCollection services, IBlobStorage storage)
        {
            services.AddSingleton<IStorageNetOssStore, StorageNetOssStore>(sp => new StorageNetOssStore(storage));
            services.AddSingleton<IOpenStoreOss>(sp => sp.GetRequiredService<IStorageNetOssStore>());
        }
    }
}