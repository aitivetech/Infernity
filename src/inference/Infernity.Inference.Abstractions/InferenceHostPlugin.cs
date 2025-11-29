using System.Text.Json.Serialization;

using Infernity.Framework.Core.Collections;
using Infernity.Framework.Core.Reflection;
using Infernity.Framework.Json.Converters;
using Infernity.Framework.Plugins.Host;
using Infernity.Inference.Abstractions.Models;
using Infernity.Inference.Abstractions.Models.Manifest;
using Infernity.Inference.Abstractions.Models.Manifest.Serialization;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infernity.Inference.Abstractions;

public sealed class InferenceHostPlugin : IHostPlugin
{
    public void ConfigureHost(IHostApplicationBuilder applicationBuilder)
    {
        applicationBuilder.Services.AddSingleton<JsonConverter, ModelManifestJsonConverter>();
        applicationBuilder.Services.AddSingleton<JsonConverter>(sp =>
            new PolymorphicResolverJsonConverter<InferenceProviderId, InferenceProviderConfiguration>(
                new DictionaryTypeResolver<InferenceProviderId, IInferenceProviderFactory>(
                    sp.GetServices<IInferenceProviderFactory>(),
                    i => i.Id,
                    i => i.ConfigurationSectionType)));

        applicationBuilder.Services.AddSingleton<JsonConverter, ModelIdJsonConverter>();
        applicationBuilder.Services.AddSingleton<JsonConverter, ModelArchitectureIdJsonConverter>();
        applicationBuilder.Services.AddSingleton<JsonConverter, ModelFamilyIdJsonConverter>();
        applicationBuilder.Services.AddSingleton<JsonConverter, InferenceProviderIdJsonConverter>();
        
        applicationBuilder.Services.AddSingleton<IInferenceProvider>(sp =>
        {
            var configuration = sp.GetRequiredService<InferenceProviderConfiguration>();
            var factories = sp.GetServices<IInferenceProviderFactory>().ToDictionary(i => i.Id);

            var provider = factories.GetOptional(configuration.Provider).OrThrow(() =>
                new InferenceException($"Unknown inference provider: {configuration.Provider}"));
            
            return provider.CreateInferenceProvider(configuration);
        });
    }
}