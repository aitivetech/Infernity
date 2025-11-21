using System.Text.Json.Nodes;

using Infernity.Framework.Core.Functional;
using Infernity.Framework.Json.Dom;

namespace Infernity.Framework.Configuration.Middleware;

public abstract class MergingConfigurationMiddleware : IConfigurationMiddleware
{
    public void Invoke(ConfigurationContext input,
        Action next)
    {
        var data = OnGetData(input);

        if (data)
        {
            if (input.Data)
            {
                input.Data = input.Data.Value.MergeFrom(data.Value);
            }
            else
            {
                input.Data = data;
            }
        }
        
        next();
    }
    
    protected abstract Optional<JsonObject> OnGetData(ConfigurationContext context);
}