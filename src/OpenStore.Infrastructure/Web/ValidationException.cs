namespace OpenStore.Infrastructure.Web;

public class ValidationException : ApplicationException
{
    public IReadOnlyCollection<ValidationException> Errors { get; } = new List<ValidationException>();

    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string message, IReadOnlyCollection<ValidationException> errors) : this(message)
    {
        Errors = errors;
    }
}