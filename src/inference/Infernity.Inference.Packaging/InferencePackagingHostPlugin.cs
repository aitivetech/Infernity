using Infernity.Framework.Plugins.Host;
using Infernity.Inference.Packaging.Builder;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infernity.Inference.Packaging;

public sealed class InferencePackagingHostPlugin : IHostPlugin
{
    public void ConfigureHost(IHostApplicationBuilder applicationBuilder)
    {
        applicationBuilder.Services.AddSingleton<ModelPackageBuilder>();
    }
}