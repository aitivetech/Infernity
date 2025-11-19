using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Infernity.Framework.Configuration;

public static class HostBuilderExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public IConfiguration CreateDefaultConfiguration(string applicationId)
        {
            var configurationName = applicationId.ToLowerInvariant();

            var baseConfigurationFileName = $"{configurationName}.json";
            var environmentConfigurationFileName =
                $"{configurationName}.{builder.Environment.EnvironmentName.ToLowerInvariant()}.json";
            
            builder.Configuration.Sources.Clear();
            builder.Configuration
                .AddJsonFile(baseConfigurationFileName,
                    false,
                    false)
                .AddJsonFile(environmentConfigurationFileName,
                    true,
                    false);
            
            return builder.Configuration;
        }
    }
}