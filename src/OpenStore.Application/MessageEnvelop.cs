using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using OpenStore.Domain;

namespace OpenStore.Application
{
    /// <summary>
    /// Stream message abstraction
    /// </summary>
    public abstract class MessageEnvelop
    {
        [Key] public Guid Id { get; protected set; }
        [Required] public string Type { get; set; }
        [Required] public string Payload { get; set; }
        [Required] public DateTimeOffset Timestamp { get; set; }
        [Required] public ulong Version { get; set; }
        public string CorrelationId { get; set; }

        protected MessageEnvelop()
        {
        }
        
        protected MessageEnvelop(object message, ulong version, string correlationId)
        {
            Id = KeyGenerator.GenerateGuid();
            Type = message.GetType().AssemblyQualifiedName;
            Payload = JsonSerializer.Serialize(message);
            Timestamp = DateTimeOffset.UtcNow;
            Version = version;
            CorrelationId = correlationId;
        }

        public object RecreateMessage() => JsonSerializer.Deserialize(Payload, System.Type.GetType(Type));
    }
}