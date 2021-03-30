using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace OpenStore.Infrastructure.Localization
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseOpenStoreLocalization(this IApplicationBuilder app)
        {
            app.UseRequestLocalization();

            var path = new PathString("/set-language");
            app.Use(async (httpContext, next) =>
            {
                if (httpContext.Request.Path.StartsWithSegments(path) && httpContext.Request.Method == "POST")
                {
                    var culture = httpContext.Request.Form["culture"];
                    var returnUrl = httpContext.Request.Form["returnUrl"];

                    httpContext.Response.Cookies.Append(
                        CookieRequestCultureProvider.DefaultCookieName,
                        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                        new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                    );

                    if (!string.IsNullOrWhiteSpace(returnUrl))
                    {
                        httpContext.Response.Redirect(returnUrl);
                    }
                }
                
                await next();
            });

            return app;
        }
    }
}