using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace OpenStore.Infrastructure.Web.Modularization
{
    public class AssemblyBasedControllerFeatureProvider : ControllerFeatureProvider
    {
        private readonly ISet<Assembly> _assemblies;

        public AssemblyBasedControllerFeatureProvider(ISet<Assembly> assemblies)
        {
            _assemblies = assemblies ?? throw new ArgumentNullException(nameof(assemblies));
        }

        protected override bool IsController(TypeInfo typeInfo) => _assemblies.Contains(typeInfo.Assembly) && base.IsController(typeInfo);
    }
}