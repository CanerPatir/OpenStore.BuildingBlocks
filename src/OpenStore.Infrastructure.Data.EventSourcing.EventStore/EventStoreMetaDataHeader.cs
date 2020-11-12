namespace OpenStore.Infrastructure.Data.EventSourcing.EventStore
{
    public class EventStoreMetaDataHeader
    {
        public string ClrType { get; set; }
        public long CommitNumber { get; set; }
    }
}