namespace OpenStore.Domain.EventSourcing.Exception
{
    public class AggregateEventOnApplyMethodMissingException : System.Exception
    {
        public AggregateEventOnApplyMethodMissingException(string message) : base(message)
        {
        }
    }
}