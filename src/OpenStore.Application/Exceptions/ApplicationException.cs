using OpenStore.Domain;

namespace OpenStore.Application.Exceptions;

public class ApplicationException : DomainException
{
    public ApplicationException()
    {
    }

    public ApplicationException(string message) : base(message)
    {
    }

    public ApplicationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}