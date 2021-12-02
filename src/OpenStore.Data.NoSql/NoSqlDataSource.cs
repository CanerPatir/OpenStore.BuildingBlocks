using System.Globalization;

namespace OpenStore.Data.NoSql;

public class NoSqlDataSource : IEquatable<NoSqlDataSource>
{
    public static readonly RavenDbSource RavenDb = new RavenDbSource();
    public static readonly CouchbaseSource Couchbase = new CouchbaseSource();
    public static readonly MongoDbSource MongoDb = new MongoDbSource();

    protected NoSqlDataSource(string name, bool transactional)
    {
        Name = name;
        Transactional = transactional;
    }

    public static NoSqlDataSource FromString(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        return value.ToLower(CultureInfo.InvariantCulture) switch
        {
            "ravendb" => NoSqlDataSource.RavenDb,
            "couchbase" => NoSqlDataSource.Couchbase,
            "mongodb" => NoSqlDataSource.MongoDb,
            _ => throw new NotSupportedException()
        };
    }

    public string Name { get; }
    public bool Transactional { get; }

    public bool Equals(NoSqlDataSource other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((NoSqlDataSource) obj);
    }

    public override int GetHashCode()
    {
        return Name != null ? Name.GetHashCode() : 0;
    }
}


public class RavenDbSource : NoSqlDataSource
{
    internal RavenDbSource() : base(nameof(RavenDb), true)
    {
    }
}

public class CouchbaseSource : NoSqlDataSource
{
    internal CouchbaseSource() : base(nameof(Couchbase), false)
    {
    }
}

public class MongoDbSource : NoSqlDataSource
{
    internal MongoDbSource() : base(nameof(MongoDb), true)
    {
    }
}