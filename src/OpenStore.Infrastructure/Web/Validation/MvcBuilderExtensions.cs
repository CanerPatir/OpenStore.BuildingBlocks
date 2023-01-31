using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace OpenStore.Infrastructure.Web.Validation;

public static class MvcBuilderExtensions
{
    public static IMvcBuilder AddOpenStoreValidation(this IMvcBuilder mvcBuilder, Action<FluentValidationAutoValidationConfiguration> configurationExpression = null)
    {
        mvcBuilder.Services.AddFluentValidationAutoValidation(configurationExpression).AddFluentValidationClientsideAdapters();
        return mvcBuilder;
    }
}