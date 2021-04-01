using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace OpenStore.Infrastructure.Localization
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseOpenStoreLocalization(this IApplicationBuilder app)
        {
            app.Map(new PathString("/set-language"), _app =>
            {
                _app.Use((httpContext, next) =>
                {
                    var culture = httpContext.Request.Form["culture"];
                    var returnUrl = httpContext.Request.Form["returnUrl"];
                    httpContext.Response.Cookies.Append(
                        LocalizationConstants.DefaultCookieName,
                        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                        new CookieOptions {Expires = DateTimeOffset.UtcNow.AddYears(1)}
                    );                  

                    if (!string.IsNullOrWhiteSpace(returnUrl))
                    {
                        httpContext.Response.Redirect(returnUrl);
                    }
                    else
                    {
                        httpContext.Response.Redirect("/");
                    }

                    // await next();
                    
                    return Task.CompletedTask;
                });
            });

            app.UseRequestLocalization();

            return app;
        }
    }
}