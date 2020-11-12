using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace OpenStore.Infrastructure.Web.Swagger
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds OpenStore module swagger services for only development purpose
        /// </summary>
        public static IServiceCollection AddOpenStoreSwaggerForModule<TModuleStartup>(this IServiceCollection services, IWebHostEnvironment environment, string name, string version = "v1")
        {
            if (!environment.IsDevelopment())
            {
                return services;
            }
            
            var assembly = typeof(TModuleStartup).Assembly;
            services.AddSwaggerGen(c =>
            {
                c.SwaggerGeneratorOptions.DocInclusionPredicate = (docName, apiDesc) => (apiDesc.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                                                                                        && controllerActionDescriptor.ControllerTypeInfo.Assembly == assembly;

                c.SwaggerDoc(name, new OpenApiInfo {Title = $"OpenStore - {name}", Version = version});
                c.AddSecurityDefinition(name, new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
            });

            return services;
        }
    }
}