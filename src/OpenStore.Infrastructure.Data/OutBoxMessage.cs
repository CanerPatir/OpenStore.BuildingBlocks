using System.ComponentModel.DataAnnotations;
using OpenStore.Application;
using OpenStore.Domain;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable VirtualMemberCallInConstructor
// ReSharper disable MemberCanBeProtected.Global

namespace OpenStore.Infrastructure.Data
{
    public class OutBoxMessage : MessageEnvelop
    {
        [Required] public bool Committed { get; set; }
        [Required] public string AggregateId { get; set; }

        public OutBoxMessage()
        {
        }

        public OutBoxMessage(IDomainEvent message) : base(message, message.Version, message.CorrelationId)
        {
            Committed = false;
            AggregateId = message.Id.ToString();
        }

        public void MarkAsCommitted()
        {
            Committed = true;
        }

        public override string ToString() => $"[Id:{Id}, Type:{Type}, Timestamp:{Timestamp}, Committed:{Committed}, Payload:\"{Payload}\", CorrelationId: \"{CorrelationId}\"]";
    }
}