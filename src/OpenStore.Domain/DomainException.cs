

// ReSharper disable MemberCanBeProtected.Global

namespace OpenStore.Domain;

public class DomainException : Exception
{
    public DomainException()
    {
    }

    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public DomainException(string message, string errorCode) : this(message)
    {
        ErrorCode = errorCode;
    }


    public DomainException(string message, string errorCode, Exception innerException) : this(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public string ErrorCode { get; }
}