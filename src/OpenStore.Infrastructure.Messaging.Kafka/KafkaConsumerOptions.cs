using Confluent.Kafka;

namespace OpenStore.Infrastructure.Messaging.Kafka;

// todo: enrich
public class KafkaConsumerOptions
{
    public string[] BootstrapServers { get; set; }

    public AutoOffsetReset AutoOffsetReset { get; set; } = AutoOffsetReset.Latest;
}