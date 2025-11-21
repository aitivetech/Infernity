using Infernity.Framework.Configuration.Default;
using Infernity.Framework.Configuration.Middleware;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

            builder.Configuration.AddEnvironmentVariables();
            
            builder.Services.AddSingleton<IConfigurationMiddleware>(sp => new JsonFileConfigurationMiddleware(
                baseConfigurationFileName,
                100));
            
            builder.Services.AddSingleton<IConfigurationMiddleware>(sp => new JsonFileConfigurationMiddleware(
                environmentConfigurationFileName,
                101));
            
            builder.Services.AddSingleton<IConfigurationReader, DefaultConfigurationReader>();
            
            return builder.Configuration;
        }
    }
}