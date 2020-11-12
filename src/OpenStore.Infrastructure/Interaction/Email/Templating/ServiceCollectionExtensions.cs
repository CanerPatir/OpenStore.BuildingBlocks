using System;
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
        
        public static MailBuilder UseTemplate<TModel>(this MailBuilder mailBuilder, IServiceProvider serviceProvider, string templateViewPath, TModel model)
        {
            var razorViewToStringRenderer = serviceProvider.GetRequiredService<IRazorViewToStringRenderer>();
            mailBuilder.UseHtmlBody().Body(razorViewToStringRenderer.RenderViewToStringAsync(templateViewPath, model).Result);
            return mailBuilder;
        }
    }
}