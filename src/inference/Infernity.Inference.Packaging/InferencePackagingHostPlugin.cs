using Infernity.Framework.Plugins.Host;
using Infernity.Inference.Abstractions.Models;
using Infernity.Inference.Packaging.Nuget;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infernity.Inference.Packaging;

public sealed class InferencePackagingHostPlugin : IHostPlugin
{
    public void ConfigureHost(IHostApplicationBuilder applicationBuilder)
    {
        applicationBuilder.Services.AddSingleton<ModelPackageBuilder>();
        applicationBuilder.Services.AddSingleton<IModelFeed, NugetPackageFeed>();
    }
}