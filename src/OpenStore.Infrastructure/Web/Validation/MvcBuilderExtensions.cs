using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace OpenStore.Infrastructure.Web.Validation;

public static class MvcBuilderExtensions
{
    public static IMvcBuilder AddOpenStoreValidation(this IMvcBuilder mvcBuilder, Action<FluentValidationMvcConfiguration> configurationExpression = null)
    {
        return mvcBuilder.AddFluentValidation(configurationExpression);
    }
}