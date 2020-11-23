using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Application.Email;

namespace OpenStore.Infrastructure.Email.Templating
{
    public static class ServiceCollectionExtensions
    {
        public static async Task<MailBuilder> UseTemplate<TModel>(this MailBuilder mailBuilder, IServiceProvider serviceProvider, string templateViewPath, TModel model)
        {
            var razorViewToStringRenderer = serviceProvider.GetRequiredService<IRazorViewToStringRenderer>();
            mailBuilder.UseHtmlBody().Body(await razorViewToStringRenderer.RenderViewToStringAsync(templateViewPath, model));
            return mailBuilder;
        }
    }
}