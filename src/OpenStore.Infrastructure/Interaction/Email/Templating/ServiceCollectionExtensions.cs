using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace OpenStore.Infrastructure.Interaction.Email.Templating
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRazorEmailTemplateEngine(this IServiceCollection services)
        {
            services.AddTransient<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
            return services;
        }
        
        public static async Task<MailBuilder> UseTemplate<TModel>(this MailBuilder mailBuilder, IServiceProvider serviceProvider, string templateViewPath, TModel model)
        {
            var razorViewToStringRenderer = serviceProvider.GetRequiredService<IRazorViewToStringRenderer>();
            mailBuilder.UseHtmlBody().Body(await razorViewToStringRenderer.RenderViewToStringAsync(templateViewPath, model));
            return mailBuilder;
        }
    }
}