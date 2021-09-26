using System.ComponentModel.DataAnnotations;
using OpenStore.Domain;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable VirtualMemberCallInConstructor
// ReSharper disable MemberCanBeProtected.Global

namespace OpenStore.Data.OutBox
{
    public class OutBoxMessage : MessageEnvelop
    {
        [Required] public bool Committed { get; protected set; }
        [Required] public string AggregateId { get; protected set; }

        protected OutBoxMessage()
        {
        }

        public OutBoxMessage(IDomainEvent message, string correlationId, string committedBy) : base(message, message.Version, correlationId, committedBy)
        {
            AggregateId = message.Id;
        }

        public void MarkAsCommitted()
        {
            Committed = true;
        }

        public override string ToString() => $"[Id:{Id}, Type:{Type}, Timestamp:{Timestamp}, Committed:{Committed}, Payload:\"{Payload}\", CorrelationId: \"{CorrelationId}\"]";
    }
}