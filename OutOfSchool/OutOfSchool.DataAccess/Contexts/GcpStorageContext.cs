using System;
using Google.Cloud.Storage.V1;

namespace OutOfSchool.Services.Contexts
{
    public class GcpStorageContext : IGcpStorageContext
    {
        public GcpStorageContext(StorageClient client, string bucketName)
        {
            StorageClient = client ?? throw new ArgumentNullException(nameof(client));
            BucketName = bucketName ?? throw new ArgumentNullException(nameof(bucketName));
        }

        public StorageClient StorageClient { get; }

        public string BucketName { get; }
    }
}