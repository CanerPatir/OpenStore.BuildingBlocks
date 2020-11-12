namespace OpenStore.Domain.EventSourcing.Exception
{
    public class AggregateStateMismatchException : System.Exception
    {
        public AggregateStateMismatchException(string message) : base(message)
        {
        }
    }
}