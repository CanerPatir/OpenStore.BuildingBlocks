using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Infrastructure.Interaction.Email.Smtp.MailKit;
using OpenStore.Infrastructure.Interaction.Email.Templating;

namespace OpenStore.Infrastructure.Interaction.Email
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMailInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRazorEmailTemplateEngine();
            return services.AddMailKitSmtp(configuration);
        }
    }
}