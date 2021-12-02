using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using MediatR;
using OpenStore.Shared;

namespace OpenStore.Data.OutBox;

/// <summary>
/// Stream message abstraction
/// </summary>
public abstract class MessageEnvelop : INotification
{
    [Key] public Guid Id { get; protected set; }
    [Required] public string Type { get; protected set; }
    [Required] public string Payload { get; protected set; }
    [Required] public DateTimeOffset Timestamp { get; protected set; }
    [ConcurrencyCheck] [Required] public long Version { get; protected set; }
    public string CorrelationId { get; protected set; }
    public string CommittedBy { get; protected set; }

    protected MessageEnvelop()
    {
    }

    protected MessageEnvelop(object message, long version, string correlationId, string committedBy)
    {
        Id = KeyGenerator.GenerateGuid();
        Type = message.GetType().AssemblyQualifiedName;
        Payload = JsonSerializer.Serialize(message);
        Timestamp = DateTimeOffset.UtcNow;
        Version = version;
        CorrelationId = correlationId;
        CommittedBy = committedBy;
    }

    public object RecreateMessage() =>
        JsonSerializer.Deserialize(Payload, System.Type.GetType(Type) ?? throw new InvalidOperationException("Message 'Type' should not be null"));
}