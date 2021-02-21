// ReSharper disable CheckNamespace

namespace Microsoft.AspNetCore.Http
{
    public static class HttpContextExtensions
    {
        public static string GetCurrentUrl(this HttpContext httpContext)
        {
            return $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}{httpContext.Request.QueryString}";
        }
    }
}