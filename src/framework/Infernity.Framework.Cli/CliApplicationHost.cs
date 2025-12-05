using CommandDotNet;
using CommandDotNet.DataAnnotations;
using CommandDotNet.IoC.MicrosoftDependencyInjection;
using CommandDotNet.NameCasing;
using CommandDotNet.Spectre;

using Infernity.Framework.Logging;
using Infernity.Framework.Plugins;
using Infernity.Framework.Plugins.Host;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Spectre.Console;

namespace Infernity.Framework.Cli;

public class CliApplicationHost<T> : PluginApplicationHost<IPluginBinder> 
    where T : class
{
    private readonly AppRunner<T> _appRunner;
    private readonly Action<IHost,AppConfigBuilder> _configure;
    
    public CliApplicationHost(string applicationId,
        IReadOnlyList<IPluginProvider> pluginProviders,
        Action<IHost,AppConfigBuilder>? configure = null,
        IPluginSelector? pluginSelector = null) : base(applicationId,
        Host.CreateApplicationBuilder(),
        pluginProviders,
        new HostPluginActivator(),
        ((s, level) => new DefaultLoggingBinder(s, level)),
        pluginSelector,
        false)
    {
        _appRunner = new AppRunner<T>();
        _configure = configure ??  ((host, appConfigBuilder) => { });
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
            .UseDataAnnotationValidations()
            .UseNameCasing(Case.CamelCase)
            .UseSpectreAnsiConsole()
            .Configure(b => b.AppSettings.Arguments.DefaultArgumentMode = ArgumentMode.Option)
            .Configure(b => _configure(host, b));
    }

    protected override IHost OnBuildHost(IHostApplicationBuilder builder)
    {
        return ((HostApplicationBuilder)builder).Build();
    }

    protected override void OnRegisterSystemServices(IHostApplicationBuilder builder,
        IServiceCollection services)
    {
        base.OnRegisterSystemServices(builder, services);

        services.AddSingleton<T>();
    }
}