using System;

namespace OpenStore.Domain.EventSourcing
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AggregateEventApplierAttribute : Attribute
    {
    }
}