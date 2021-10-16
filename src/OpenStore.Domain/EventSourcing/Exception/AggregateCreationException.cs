namespace OpenStore.Domain.EventSourcing.Exception;

public class AggregateCreationException : System.Exception
{
    public AggregateCreationException(string message) : base(message)
    {
    }
}