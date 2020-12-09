using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OpenStore.Infrastructure.Web.ReCaptcha
{
    /// <summary>
    /// Extension methods for adding reCAPTCHA services to the DI container.
    /// </summary>
    public static class RecaptchaServiceCollection
    {
        internal const string ReCaptchaHttpClientKey = "reCaptchaClient";

        /// <summary>
        /// Adds services required for using reCAPTCHA.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="options">The action used to configure reCAPTCHA.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddOpenStoreRecaptcha(this IServiceCollection services, Action<RecaptchaSettings> options)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var recaptchaSettings = new RecaptchaSettings();
            options.Invoke(recaptchaSettings);

            services.Configure(options);
            services.AddTransient<IRecaptchaService, RecaptchaService>();
            services.AddHttpClient(ReCaptchaHttpClientKey);

            return services;
        }

        /// <summary>
        /// Adds services required for using reCAPTCHA.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="configurationSection">The configuration section that contains reCAPTCHA settings.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddOpenStoreRecaptcha(this IServiceCollection services, IConfiguration configurationSection)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configurationSection == null)
                throw new ArgumentNullException(nameof(configurationSection));

            services.Configure<RecaptchaSettings>(configurationSection);
            services.AddTransient<IRecaptchaService, RecaptchaService>();
            services.AddHttpClient(ReCaptchaHttpClientKey);

            return services;
        }
    }
}