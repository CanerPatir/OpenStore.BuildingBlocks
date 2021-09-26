namespace OpenStore.Data.EventSourcing.EventStore
{
    public class EventStoreMetaDataHeader
    {
        public string ClrType { get; set; }
        public long CommitNumber { get; set; }
    }
}