using CommandDotNet;
using CommandDotNet.DataAnnotations;
using CommandDotNet.IoC.MicrosoftDependencyInjection;
using CommandDotNet.NameCasing;
using CommandDotNet.Spectre;

using Infernity.Framework.Plugins;
using Infernity.Framework.Plugins.Host;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Infernity.Framework.Cli;

public class CliApplicationHost<T> : PluginApplicationHost<IPluginBinder> 
    where T : class
{
    private readonly AppRunner<T> _appRunner;
    
    public CliApplicationHost(string applicationId,
        IHostApplicationBuilder builder,
        IReadOnlyList<IPluginProvider> pluginProviders,
        IPluginSelector? pluginSelector = null) : base(applicationId,
        builder,
        pluginProviders,
        new HostPluginActivator(),
        pluginSelector)
    {
        _appRunner = new AppRunner<T>();
    }

    protected override async Task OnRunHost(IHost host,
        string[] arguments,
        CancellationToken cancellationToken)
    {
       await  _appRunner.RunAsync(arguments);
    }

    protected override void OnConfigureHost(IHost host,
        IConfiguration configuration,
        IPluginBinder binder)
    {
        base.OnConfigureHost(host, configuration, binder);

        _appRunner
            .UseDefaultMiddleware()
            .UseMicrosoftDependencyInjection(host.Services)
            .UseTypoSuggestions()
            .UseDataAnnotationValidations()
            .UseNameCasing(Case.CamelCase)
            .UseSpectreAnsiConsole();
    }

    protected override IHost OnBuildHost(IHostApplicationBuilder builder)
    {
        return ((HostApplicationBuilder)builder).Build();
    }
}