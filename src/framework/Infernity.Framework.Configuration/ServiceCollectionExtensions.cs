using System.Reflection;

using Infernity.Framework.Core.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace Infernity.Framework.Configuration;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddAllConfigurationSections(Assembly assembly)
        {
            foreach (var configurationSectionDescription in
                     assembly.CreateInstanceMarkedWithAttribute<ConfigurationSectionAttribute>())
            {
                var localDescription = configurationSectionDescription;
                var sectionType = localDescription.instance.GetType();

                var serviceDescription = new ServiceDescriptor(sectionType,
                    sp =>
                    {
                        var reader = sp.GetRequiredService<IConfigurationReader>();
                        return reader.ReadRequired(
                            sectionType);
                    },
                    ServiceLifetime.Singleton);
                
                services.Add(serviceDescription);
            }
        }
    }
}