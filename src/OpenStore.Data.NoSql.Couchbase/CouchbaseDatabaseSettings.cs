namespace OpenStore.Data.NoSql.Couchbase;

public class CouchbaseDatabaseSettings
{
    internal CouchbaseDatabaseSettings(string bucketName)
    {
        BucketName = bucketName;
    }

    public string BucketName { get; }
}