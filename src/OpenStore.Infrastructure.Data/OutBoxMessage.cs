using System;
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
        [Required] public bool Committed { get; protected set; }
        [Required] public string AggregateId { get; protected set; }

        protected OutBoxMessage()
        {
        }

        public OutBoxMessage(IDomainEvent message, string correlationId) : base(message, message.Version, correlationId)
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