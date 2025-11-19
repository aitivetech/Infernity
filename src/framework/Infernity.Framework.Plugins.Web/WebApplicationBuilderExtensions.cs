using Infernity.Framework.Plugins.Selectors;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Infernity.Framework.Plugins.Web;

public static class WebApplicationBuilderExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public IWebPluginBinder AddPlugins(IReadOnlyList<IPluginProvider> providers,IPluginSelector? selector = null)
        {
            var actualSelector = selector ?? new DelegatePluginSelector(p => true);
            var activator = new WebPluginActivator(builder.Services);

            var pluginManager = IPluginManager.Create(builder.Environment,
                providers,
                activator,
                actualSelector);
            
            builder.Services.AddSingleton<IPluginManager>(pluginManager);

            return activator.Build();
        }
    }
}