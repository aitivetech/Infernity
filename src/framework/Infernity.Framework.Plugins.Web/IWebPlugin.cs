using Microsoft.AspNetCore.Builder;

namespace Infernity.Framework.Plugins.Web;

public interface IWebPlugin
{
    void Configure(WebApplication application);
}