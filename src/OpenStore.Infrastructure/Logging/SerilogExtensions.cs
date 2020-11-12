using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace OpenStore.Infrastructure.Logging
{
    public static class SerilogExtensions
    {
        // TODO: important!! fix module settings for separated service scenarios
        public static IHostBuilder AddOpenStoreLogging(this IHostBuilder hostBuilder)
        {
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
                .Enrich.WithProperty("env", hostEnvironment.EnvironmentName)
                .Enrich.WithProperty("arch", $"isX64 os: {isX64}, isX64 process: {isX64Process}")
                .ReadFrom.Configuration(configuration);
        }
    }
}