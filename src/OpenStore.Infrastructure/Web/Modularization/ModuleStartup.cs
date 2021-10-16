using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable MemberCanBePrivate.Global

namespace OpenStore.Infrastructure.Web.Modularization;

public abstract class ModuleStartup
{
    protected IConfiguration Configuration { get; }
    protected IWebHostEnvironment Environment { get; }
    protected Assembly Assembly => GetType().Assembly;
    /// <summary>
    /// Returns true if underlying module is not a sub application
    /// </summary>
    protected bool IsSeparatedService => Assembly.GetEntryAssembly() == Assembly;
    protected virtual IEnumerable<string> ModuleSettingFileNames => Enumerable.Empty<string>();
    /// <summary>
    /// Returns route prefix of sub application. Returns null if underlying module is not a sub application
    /// </summary>
    public PathString RoutePrefix { get; internal set; }

    protected ModuleStartup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        if (environment == null) throw new ArgumentNullException(nameof(environment));

        Configuration = ModuleConfigBuilder.Build(environment, configuration, ModuleSettingFileNames);
        Environment = environment;
    }
        
    public abstract void ConfigureServices(IServiceCollection services);

    public abstract void Configure(IApplicationBuilder app, IWebHostEnvironment env);

    public async Task Start(IApplicationBuilder app, CancellationToken cancellationToken)
    {
        Configure(app, Environment);
        await OnStarting(app.ApplicationServices, cancellationToken); // module start hook to seed data for module or migrations
    }

    protected virtual Task OnStarting(IServiceProvider serviceProvider, CancellationToken cancellationToken) => Task.CompletedTask;
}