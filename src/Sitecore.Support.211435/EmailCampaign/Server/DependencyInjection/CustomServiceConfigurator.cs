using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Sitecore.Support.EmailCampaign.Server.DependencyInjection
{
    using Microsoft.Extensions.DependencyInjection;
    using Sitecore.Configuration;
    using Sitecore.DependencyInjection;
    using Sitecore.EmailCampaign.Server.DependencyInjection;
    using Sitecore.EmailCampaign.Server.Controllers.Dispatch.ButtonStates;
    using Sitecore.EmailCampaign.Server.Helpers;
    using Sitecore.EmailCampaign.Server.Model;
    using Sitecore.EmailCampaign.Server.Services;
    using Sitecore.EmailCampaign.Server.Services.Interfaces;
    using Sitecore.ExM.Framework.Formatters;
    using Sitecore.Modules.EmailCampaign.Core.Dispatch;
    using Sitecore.Services.Infrastructure.Sitecore.DependencyInjection;

    internal class CustomServiceConfigurator : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            /*Type type = Type.GetType("Sitecore.EmailCampaign.Server.DependencyInjection.CustomServiceConfigurator");
            MethodInfo info = type.GetMethod("Configure", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            info.Invoke(this, new object[] { serviceCollection });*/
            //var dllFile = new FileInfo(@"/bin/Sitecore.EmailCampaign.Server.dll");
            /*var dllFile = MainUtil.MapPath("/bin/Sitecore.EmailCampaign.Server.dll");
            var assembly = Assembly.LoadFile(dllFile);
            Type type = assembly.GetType("Sitecore.EmailCampaign.Server.DependencyInjection.CustomServiceConfigurator");

            if (type != null)
            {
                MethodInfo methodInfo = type.GetMethod("Configure");

                if (methodInfo != null)
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    object classInstance = Activator.CreateInstance(type, null);

                    if (parameters.Length == 0)
                    {
                        // This works fine
                        methodInfo.Invoke(classInstance, null);
                    }
                    else
                    {
                        object[] parametersArray = new object[] { serviceCollection };

                        // The invoke does NOT work;
                        // it throws "Object does not match target type"             
                        methodInfo.Invoke(classInstance, parametersArray);
                    }
                }
            }*/
            Assembly[] assemblies = new Assembly[]
            {
            base.GetType().Assembly
            };
            serviceCollection.AddWebApiControllers(assemblies);

        }
    }
}
