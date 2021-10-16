using System.Text;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable MemberCanBePrivate.Global

namespace OpenStore.Infrastructure.Web;

// [Authorize]
[ApiController]
[Route("[controller]")]
public abstract class BaseApiController : BaseController
{
    protected ValidationException GetApplicationExceptionFromModelState()
    {
        if (ModelState.IsValid) return null;

        var errors = ModelState.Values.SelectMany(x => x.Errors).ToList();
        var sb = new StringBuilder();
        foreach (var modelError in errors)
        {
            if (errors.Last() == modelError)
            {
                sb.Append(modelError.ErrorMessage);
            }
            else
            {
                sb.AppendLine(modelError.ErrorMessage);
            }
        }

        return new ValidationException(sb.ToString(), errors.Select(x => new ValidationException(x.ErrorMessage)).ToList());
    }

    protected void ThrowIfModelInvalid()
    {
        var applicationException = GetApplicationExceptionFromModelState();
        if (applicationException != null)
        {
            throw applicationException;
        }
    }

    protected static Task NotSupported() => throw new NotSupportedException("Operation not supported");
}