using Infernity.Framework.Plugins.Selectors;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infernity.Framework.Plugins;

public static class HostApplicationBuilderExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public TBinder AddPlugins<TBinder>(IReadOnlyList<IPluginProvider> providers,IPluginActivator<TBinder> activator,IPluginSelector? selector = null)
            where TBinder : IPluginBinder
        {
            var actualSelector = selector ?? new DelegatePluginSelector(p => true);

            var pluginManager = IPluginManager<TBinder>.Create(builder,
                providers,
                activator,
                actualSelector);
            
            builder.Services.AddSingleton<IPluginManager<TBinder>>(pluginManager);

            return pluginManager.Build();
        }
    }
}