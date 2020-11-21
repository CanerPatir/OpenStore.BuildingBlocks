using System.Collections.Generic;

namespace OpenStore.Infrastructure.Web.ErrorHandling
{
    internal record OpenStoreWebErrorDto(string Message, IEnumerable<string> Errors);
}