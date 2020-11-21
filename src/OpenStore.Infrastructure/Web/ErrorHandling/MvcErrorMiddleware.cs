using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace OpenStore.Infrastructure.Web.ErrorHandling
{
    public class MvcErrorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly PathString _errorHandlingPath;

        public MvcErrorMiddleware(RequestDelegate next, PathString errorHandlingPath)
        {
            _next = next;
            _errorHandlingPath = errorHandlingPath;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            Exception ex = null;
            try
            {
                await _next(context);
            }
            catch (Exception e)
            {
                ex = e;
            }

            if (ex == null) return;

            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("UseOpenStoreApiExceptionHandling");
            logger.LogError(ex.Demystify(), "OpenStore http request error");
            
            context.Response.Redirect(_errorHandlingPath);
        }
    }
}