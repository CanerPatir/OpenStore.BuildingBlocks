namespace OpenStore.Data.NoSql.RavenDb
{
    public class RavenDatabaseSettings
    {
        public string[] Urls { get; set; }
        public string DatabaseName { get; set; }
        public string CertPath { get;set; }
        public string CertPass { get;set; }
    }
}