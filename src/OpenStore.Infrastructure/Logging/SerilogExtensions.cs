using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace OpenStore.Infrastructure.Logging;

public static class SerilogExtensions
{
    // TODO: important!! fix module settings for separated service scenarios
    public static WebApplicationBuilder AddOpenStoreLogging(this WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.WebHost.ConfigureLogging((context, logging) => logging.ClearProviders());

        webApplicationBuilder.WebHost.UseSerilog((context, loggerConfiguration) => loggerConfiguration.AddDefaults(context.Configuration, context.HostingEnvironment));

        return webApplicationBuilder;
    }

    public static IHostBuilder AddOpenStoreLogging(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureLogging((context, logging) => logging.ClearProviders());

        return hostBuilder.UseSerilog((context, loggerConfiguration) => loggerConfiguration.AddDefaults(context.Configuration, context.HostingEnvironment));
    }

    // public static IWebHostBuilder AddOpenStoreLogging(this IWebHostBuilder webHostBuilder)
    // {
    //     return webHostBuilder.UseSerilog((context, loggerConfiguration) => loggerConfiguration.AddDefaults(context.Configuration, context.HostingEnvironment));
    // }

    private static LoggerConfiguration AddDefaults(this LoggerConfiguration loggerConfiguration, IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        var isX64 = Environment.Is64BitOperatingSystem;
        var isX64Process = Environment.Is64BitProcess;

        return loggerConfiguration
            .Enrich.FromLogContext()
            .Enrich.WithProperty("version", Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version)
            .Enrich.WithProperty("env", hostEnvironment.EnvironmentName)
            .Enrich.WithProperty("arch", $"isX64 os: {isX64}, isX64 process: {isX64Process}")
            .ReadFrom.Configuration(configuration);
    }
}