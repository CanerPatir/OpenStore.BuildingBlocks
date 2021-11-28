namespace OpenStore.Domain;

public class ConcurrencyException : Exception
{
    public ConcurrencyException(string msg, Exception innerException = null) : base(msg, innerException)
    {
    }
}