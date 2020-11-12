using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ApplicationException = OpenStore.Application.Exceptions.ApplicationException;
// ReSharper disable MemberCanBePrivate.Global

namespace OpenStore.Infrastructure.Web
{
    // [Authorize]
    [ApiController]
    [Route("[controller]")]
    public abstract class BaseApiController : BaseController
    {
        protected ApplicationException GetApplicationExceptionFromModelState()
        {
            if (ModelState.IsValid) return null;

            var errors = ModelState.Values.SelectMany(x => x.Errors).ToList();
            var sb = new StringBuilder();
            foreach (var modelError in errors) sb.AppendLine(modelError.ErrorMessage);
            return new ApplicationException($"Validation error. {sb}", new AggregateException("Validation exceptions", errors.Select(x => x.Exception)));
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
}