using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Infernity.Framework.Plugins.Web;

public interface IWebPlugin
{
    void Configure(IEndpointRouteBuilder routes);
}