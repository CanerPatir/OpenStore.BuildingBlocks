// ReSharper disable CheckNamespace

using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Http;

public static class HttpContextExtensions
{
    public static string GetCurrentUrl(this HttpContext httpContext)
    {
        return $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}{httpContext.Request.QueryString}";
    }

    public static string GetUserAgent(this HttpContext httpContext)
    {
        var userAgentHeader = httpContext.Request.Headers[HeaderNames.UserAgent];

        return userAgentHeader == StringValues.Empty ? null : userAgentHeader.ToString();
    }
}