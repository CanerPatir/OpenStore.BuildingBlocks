using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;

namespace OpenStore.Infrastructure.Web.Modularization
{
    public static class SubApplicationExtensions
    {
        public static IApplicationBuilder UseSubApplication<TStartup>(this IApplicationBuilder app, PathString path)
            where TStartup : ModuleStartup
        {
            if (path.HasValue && path.Value.EndsWith("/", StringComparison.Ordinal))
            {
                throw new ArgumentException("The path must not end with a '/'", nameof(path));
            }

            var env = app.ApplicationServices.GetService<IWebHostEnvironment>();
            var configuration = app.ApplicationServices.GetService<IConfiguration>();
            var subAppStartup = (TStartup) Activator.CreateInstance(typeof(TStartup), new object[]
            {
                configuration,
                env
            });
            
            var prop = subAppStartup.GetType().GetProperty(nameof(ModuleStartup.RoutePrefix), BindingFlags.Public | BindingFlags.Instance);
            if(null != prop && prop.CanWrite)
            {
                prop.SetValue(subAppStartup, path, null);
            }
            
            if (subAppStartup == null) throw new Exception($"Module startup could not be activated. Startup: {typeof(TStartup).FullName}");
            return app.Isolate(
                builder => builder.Map(path, subApp => { subAppStartup.Start(subApp, app.ApplicationServices.GetService<IHostApplicationLifetime>().ApplicationStopping).ConfigureAwait(false).GetAwaiter().GetResult(); }), services =>
                {
                    subAppStartup.ConfigureServices(services);

                    return services.BuildServiceProvider();
                });
        }

        /// <summary>
        /// Creates a new isolated application builder which gets its own <see cref="ServiceCollection"/>, which only
        /// has the default services registered. It will not share the <see cref="ServiceCollection"/> from the
        /// originating app.
        /// </summary>
        /// <param name="app">The application builder to create the isolated app from.</param>
        /// <param name="configuration">The branch of the isolated app.</param>
        /// <param name="configureServices">A method to configure the newly created service collection.</param>
        /// <returns>The new pipeline with the isolated application integrated.</returns>
        public static IApplicationBuilder Isolate(
            this IApplicationBuilder app,
            Action<IApplicationBuilder> configuration,
            Func<IServiceCollection, IServiceProvider> configureServices)
        {
            var services = CreateDefaultServiceCollection(app.ApplicationServices);
            var branchedServiceProvider = configureServices(services);

            var branchedApplicationBuilder = new ApplicationBuilder(branchedServiceProvider);
            branchedApplicationBuilder.EnableDependencyInjection(branchedServiceProvider);

            configuration(branchedApplicationBuilder);
            branchedApplicationBuilder.RunHostedServices().GetAwaiter().GetResult();

            return app.Use(next =>
            {
                // Run the rest of the pipeline in the original context,
                // with the services defined by the parent application builder.
                branchedApplicationBuilder.Run(async context =>
                {
                    var factory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();

                    try
                    {
                        using var scope = factory.CreateScope();
                        context.RequestServices = scope.ServiceProvider;
                        await next(context);
                    }

                    finally
                    {
                        context.RequestServices = null;
                    }
                });

                var branch = branchedApplicationBuilder.Build();

                return context => branch(context);
            });
        }

        private static IApplicationBuilder EnableDependencyInjection(this IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            app.Use(async (branchContext, next) =>
            {
                var factory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

                // Store the original request services in the current ASP.NET context.
                branchContext.Items[typeof(IServiceProvider)] = branchContext.RequestServices;

                try
                {
                    using var scope = factory.CreateScope();
                    branchContext.RequestServices = scope.ServiceProvider;
                    await next();
                }

                finally
                {
                    branchContext.RequestServices = null;
                }
            });

            return app;
        }
        
        /// <summary>
        /// This creates a new <see cref="ServiceCollection"/> with the same services registered as the
        /// <see cref="WebHostBuilder"/> does when creating a new <see cref="ServiceCollection"/>.
        /// </summary>
        /// <param name="provider">The service provider used to retrieve the default services.</param>
        /// <returns>A new <see cref="ServiceCollection"/> with the default services registered.</returns>
        private static ServiceCollection CreateDefaultServiceCollection(IServiceProvider provider)
        {
            var services = new ServiceCollection();

            // Copy the services added by the hosting layer (WebHostBuilder.BuildHostingServices).
            // See https://github.com/aspnet/Hosting/blob/dev/src/Microsoft.AspNetCore.Hosting/WebHostBuilder.cs.

            services.AddLogging();

            if (provider.GetService<IHttpContextAccessor>() != null)
            {
                services.AddSingleton(provider.GetService<IHttpContextAccessor>());
            }

            services.AddSingleton(provider.GetRequiredService<IWebHostEnvironment>());
            services.AddSingleton(provider.GetRequiredService<IHostEnvironment>());

            services.AddSingleton(provider.GetRequiredService<ILoggerFactory>());
            services.AddSingleton(provider.GetRequiredService<IHostApplicationLifetime>());
            services.AddSingleton(_ => provider.GetRequiredService<IHttpContextFactory>());

            services.AddSingleton(provider.GetRequiredService<DiagnosticSource>());
            services.AddSingleton(provider.GetRequiredService<DiagnosticListener>());
            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.AddSingleton<HostedServiceExecutor>();

            services.AddSingleton(provider.GetRequiredService<IConfiguration>());

            return services;
        }
    }

    internal class HostedServiceExecutor
    {
        private readonly IEnumerable<IHostedService> _services;
        private readonly ILogger<HostedServiceExecutor> _logger;

        public HostedServiceExecutor(
            ILogger<HostedServiceExecutor> logger,
            IEnumerable<IHostedService> services)
        {
            _logger = logger;
            _services = services;
        }

        public Task StartAsync(CancellationToken token)
        {
            return ExecuteAsync(service => service.StartAsync(token));
        }

        public Task StopAsync(CancellationToken token)
        {
            return ExecuteAsync(service => service.StopAsync(token), throwOnFirstFailure: false);
        }

        private async Task ExecuteAsync(Func<IHostedService, Task> callback, bool throwOnFirstFailure = true)
        {
            List<Exception> exceptions = null;

            foreach (var service in _services)
            {
                try
                {
                    await callback(service);
                }
                catch (Exception ex)
                {
                    if (throwOnFirstFailure)
                    {
                        throw;
                    }

                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }

                    exceptions.Add(ex);
                }
            }

            // Throw an aggregate exception if there were any exceptions
            if (exceptions != null)
            {
                throw new AggregateException(exceptions);
            }
        }
    }
    
    internal static class HostedServiceExtensions
    {
        public static Task RunHostedServices(this IApplicationBuilder app)
        {
            var hostedServices = app.ApplicationServices.GetRequiredService<HostedServiceExecutor>();
            var lifeTime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
            lifeTime.ApplicationStopping.Register(() => { hostedServices.StopAsync(CancellationToken.None).GetAwaiter().GetResult(); });
            return hostedServices.StartAsync(lifeTime.ApplicationStopping);
        }
    }
}