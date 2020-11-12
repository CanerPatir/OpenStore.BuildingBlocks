using Microsoft.AspNetCore.Builder;

namespace OpenStore.Infrastructure.Localization
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseOpenStoreLocalization(this IApplicationBuilder app)
        {
            return app.UseRequestLocalization();
        }
    }
}