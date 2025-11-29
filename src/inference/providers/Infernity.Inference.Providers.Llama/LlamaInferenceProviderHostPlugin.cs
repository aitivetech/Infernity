using Infernity.Framework.Plugins.Host;
using Infernity.Inference.Abstractions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infernity.Inference.Providers.Llama;

public class LlamaInferenceProviderHostPlugin : IHostPlugin
{
    public void ConfigureHost(IHostApplicationBuilder applicationBuilder)
    {
        applicationBuilder.Services.AddSingleton<IInferenceProviderFactory, LlamaInferenceProviderFactory>();
    }
}