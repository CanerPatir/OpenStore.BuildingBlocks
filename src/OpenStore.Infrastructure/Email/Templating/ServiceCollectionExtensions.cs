using Microsoft.Extensions.DependencyInjection;
using OpenStore.Application.Email;

namespace OpenStore.Infrastructure.Email.Templating;

public static class ServiceCollectionExtensions
{
    public static async Task<MailBuilder> UseTemplate<TModel>(this MailBuilder mailBuilder, IServiceProvider serviceProvider, string templateKey, TModel model)
    {
        var razorViewToStringRenderer = serviceProvider.GetRequiredService<IRazorViewToStringRenderer>();
        mailBuilder.UseHtmlBody().Body(await razorViewToStringRenderer.RenderViewToStringAsync(templateKey, model));
        return mailBuilder;
    }
}