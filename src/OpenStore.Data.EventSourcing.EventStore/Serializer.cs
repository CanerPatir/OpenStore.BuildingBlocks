using System.Text;
using System.Text.Json;

namespace OpenStore.Data.EventSourcing.EventStore;

public class Serializer : ISerializer
{
    private Lazy<JsonSerializerOptions> DefaultSerializerSettings => new(GetSerializerSettings);

    private JsonSerializerOptions GetSerializerSettings()
    {
        return new JsonSerializerOptions
        {
        };
    }

    public byte[] Serialize(object data) =>
        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data, DefaultSerializerSettings.Value));

    public T Deserialize<T>(byte[] data) =>
        JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(data), DefaultSerializerSettings.Value);

    public object Deserialize(byte[] data, Type returnType) =>
        JsonSerializer.Deserialize(Encoding.UTF8.GetString(data), returnType, DefaultSerializerSettings.Value);
}