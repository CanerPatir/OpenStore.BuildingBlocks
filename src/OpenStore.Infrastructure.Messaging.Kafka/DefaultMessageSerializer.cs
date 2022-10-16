using System.Text.Json;
using Confluent.Kafka;

namespace OpenStore.Infrastructure.Messaging.Kafka;

public class DefaultMessageSerializer<T> : ISerializer<T>, IDeserializer<T>
    where T : class
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions();

    public byte[] Serialize(T data, SerializationContext context)
    {
        if (typeof(T) == typeof(string))
        {
            return Serializers.Utf8.Serialize(data as string, context);
        }

        return JsonSerializer.SerializeToUtf8Bytes(data, Options);
    }

    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (typeof(T) == typeof(string))
        {
            return Deserializers.Utf8.Deserialize(data, isNull, context) as T;
        }

        return isNull ? default : JsonSerializer.Deserialize<T>(data, Options);
    }
}