using System.Reflection;

using FastEndpoints;

using Infernity.Framework.Configuration;
using Infernity.Framework.Core;
using Infernity.Framework.Core.Exceptions;
using Infernity.Framework.Core.Exceptions.Default;
using Infernity.Framework.Core.Functional;
using Infernity.Framework.Core.Patterns.Disposal;
using Infernity.Framework.Core.Startup;
using Infernity.Framework.Logging;
using Infernity.Framework.Plugins;
using Infernity.Framework.Plugins.Providers;
using Infernity.Framework.Plugins.Selectors;
using Infernity.Framework.Plugins.Web;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog.Events;

namespace Infernity.Framework.Web;

public class WebApplicationHost : PluginApplicationHost<IWebPluginBinder>
{
    private readonly bool _enableMvc;
    private readonly bool _enableFastEndpoints;
    
    public WebApplicationHost(
        string applicationId,
        IReadOnlyList<IPluginProvider> pluginProviders,
        bool enableMvc = true,
        bool enableFastEndpoints = true,
        IPluginSelector? pluginSelector = null) : base(applicationId,
        WebApplication.CreateBuilder(),
        pluginProviders,
        new WebPluginActivator(),
        ((id, level) =>  new ConfigurationLoggingBinder(id, level)),
        pluginSelector)
    {
        _enableMvc = enableMvc;
        _enableFastEndpoints = enableFastEndpoints;
    }

    protected override IHost OnBuildHost(IHostApplicationBuilder builder)
    {
        return ((WebApplicationBuilder)builder).Build();
    }

    protected override IWebPluginBinder OnConfigureHostBuilder(IHostApplicationBuilder builder,
        IConfiguration configuration)
    {
        var pluginBinder = base.OnConfigureHostBuilder(builder, configuration);

        if (_enableFastEndpoints)
        {
            builder.Services.AddFastEndpoints(options => pluginBinder.Configure(options));
        }
        
        return pluginBinder;
    }

    protected override void OnConfigureHost(IHost host,
        IConfiguration configuration,
        IWebPluginBinder binder)
    {
        base.OnConfigureHost(host, configuration, binder);
        
        var webApplication = (WebApplication)host;
        
        binder.Configure(webApplication);
        
        OnConfigureWebApplication(webApplication);
    }

    protected virtual void OnConfigureWebApplication(WebApplication application)
    {
        if (_enableFastEndpoints)
        {
            application.UseFastEndpoints(config =>
            {
                
            });
        }

        if (_enableMvc)
        {
            application.MapControllers();
        }
    }
}
