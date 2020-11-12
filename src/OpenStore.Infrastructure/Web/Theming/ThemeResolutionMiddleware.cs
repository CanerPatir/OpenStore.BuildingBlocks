using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenStore.Infrastructure.Web.Theming
{
    public class ThemeResolutionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOptions<RazorPagesOptions> _razorOptions;
        private readonly ILogger<ThemeResolutionMiddleware> _logger;

        public ThemeResolutionMiddleware(
            RequestDelegate next,
            ILogger<ThemeResolutionMiddleware> logger, IOptions<RazorPagesOptions> razorOptions)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _razorOptions = razorOptions ?? throw new ArgumentNullException(nameof(razorOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Invoke(HttpContext context, IThemeResolver themeResolver)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (themeResolver == null) throw new ArgumentNullException(nameof(themeResolver));

            _logger.LogDebug("Resolving ThemeContext using {loggerType}.", themeResolver.GetType().Name);

            var themeContext = await themeResolver.ResolveAsync(context);

            if (themeContext != null)
            {
                _logger.LogDebug("ThemeContext Resolved. Adding to HttpContext.");
                context.SetThemeContext(themeContext);
            }
            else
            {
                _logger.LogDebug("ThemeContext Not Resolved.");
            }

            await _next.Invoke(context);
        }
    }
}