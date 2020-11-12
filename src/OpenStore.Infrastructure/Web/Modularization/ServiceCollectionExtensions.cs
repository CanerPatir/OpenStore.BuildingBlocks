using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace OpenStore.Infrastructure.Web.Modularization
{
    public static class ServiceCollectionExtensions
    {
        
        /// <summary>
        /// Loads controllers with its views just belong given assembly. It is useful for monolith deployment scenario
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assemblies">Assembly that belong module</param>
        /// <returns></returns>
        public static IMvcBuilder AddControllersWithViewsForAssemblies(this IServiceCollection services, params Assembly[] assemblies)
        {
            var mvcBuilder = services.AddControllersWithViews();
           
            if (!assemblies.Any()) return mvcBuilder;
            
            return ReplaceControllerFeature(new AssemblyBasedControllerFeatureProvider(new HashSet<Assembly>(assemblies)), mvcBuilder) ;
        }
        
        /// <summary>
        /// Loads controllers just belong given assembly. It is useful for monolith deployment scenario
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assemblies">Assembly that belong module</param>
        /// <returns></returns>
        public static IMvcBuilder AddControllersForAssemblies(this IServiceCollection services, params Assembly[] assemblies)
        {
            var mvcBuilder = services.AddControllers();
           
            if (!assemblies.Any()) return mvcBuilder;
            
            return ReplaceControllerFeature(new AssemblyBasedControllerFeatureProvider(new HashSet<Assembly>(assemblies)), mvcBuilder);
        }

        private static IMvcBuilder ReplaceControllerFeature(IApplicationFeatureProvider feature, IMvcBuilder mvcBuilder)
        {
            var controllerFeatureProvider = mvcBuilder.PartManager.FeatureProviders.SingleOrDefault(x => x is ControllerFeatureProvider);

            if (controllerFeatureProvider != null)
            {
                mvcBuilder.PartManager.FeatureProviders.Remove(controllerFeatureProvider);
            }

            mvcBuilder.PartManager.FeatureProviders.Add(feature);

            return mvcBuilder;
        }

    }
}