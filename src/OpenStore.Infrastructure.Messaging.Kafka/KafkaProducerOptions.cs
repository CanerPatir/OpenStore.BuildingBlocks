using Confluent.Kafka;

namespace OpenStore.Infrastructure.Messaging.Kafka;

// todo: enrich
public class KafkaProducerOptions
{
    public string[] BootstrapServers { get; set; }
    public CompressionType CompressionType { get; set; } = CompressionType.Gzip;
}