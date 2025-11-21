using Infernity.Framework.Core.Pipelines;

namespace Infernity.Framework.Configuration;

public interface IConfigurationMiddleware : IPipelineStep<ConfigurationContext>
{
    
}